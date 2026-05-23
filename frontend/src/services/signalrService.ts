import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { getToken } from './api';
import type { AvailabilityUpdateEvent } from '../types';

const HUB_URL =
  (import.meta.env.VITE_SIGNALR_HUB_URL as string) || 'http://localhost:5000/hubs/resource';

class SignalRService {
  private connection: HubConnection | null = null;
  private joined = new Set<string>();

  async connect(): Promise<HubConnection> {
    if (this.connection && this.connection.state === HubConnectionState.Connected) {
      return this.connection;
    }
    this.connection = new HubConnectionBuilder()
      .withUrl(HUB_URL, { accessTokenFactory: () => getToken() ?? '' })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    this.connection.onreconnected(async () => {
      for (const id of this.joined) {
        try { await this.connection!.invoke('JoinHospitalGroup', id); } catch { /* noop */ }
      }
    });

    await this.connection.start();
    return this.connection;
  }

  async joinHospital(hospitalId: number) {
    const conn = await this.connect();
    const id = String(hospitalId);
    if (!this.joined.has(id)) {
      await conn.invoke('JoinHospitalGroup', id);
      this.joined.add(id);
    }
  }

  async leaveHospital(hospitalId: number) {
    if (!this.connection) return;
    const id = String(hospitalId);
    if (this.joined.has(id)) {
      try { await this.connection.invoke('LeaveHospitalGroup', id); } catch { /* noop */ }
      this.joined.delete(id);
    }
  }

  onAvailabilityUpdated(handler: (e: AvailabilityUpdateEvent) => void) {
    if (!this.connection) return () => {};
    this.connection.on('AvailabilityUpdated', handler);
    return () => this.connection?.off('AvailabilityUpdated', handler);
  }

  async disconnect() {
    if (this.connection) {
      try { await this.connection.stop(); } catch { /* noop */ }
      this.connection = null;
      this.joined.clear();
    }
  }
}

export const signalrService = new SignalRService();
