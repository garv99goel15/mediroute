import { useEffect } from 'react';
import { signalrService } from '../services/signalrService';
import type { AvailabilityUpdateEvent } from '../types';

/**
 * Subscribe to live availability updates for a hospital.
 * Joins/leaves the hub group automatically on mount/unmount and id change.
 */
export function useSignalR(hospitalId: number | null, onUpdate: (e: AvailabilityUpdateEvent) => void) {
  useEffect(() => {
    if (hospitalId == null) return;
    let unsub: (() => void) | undefined;
    let cancelled = false;

    (async () => {
      try {
        await signalrService.connect();
        if (cancelled) return;
        unsub = signalrService.onAvailabilityUpdated(onUpdate);
        await signalrService.joinHospital(hospitalId);
      } catch (e) {
        console.warn('SignalR connect failed', e);
      }
    })();

    return () => {
      cancelled = true;
      if (unsub) unsub();
      signalrService.leaveHospital(hospitalId).catch(() => undefined);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [hospitalId]);
}

/**
 * Subscribe to updates for many hospitals at once (used on patient search results page).
 */
export function useMultiHospitalSignalR(
  hospitalIds: number[],
  onUpdate: (e: AvailabilityUpdateEvent) => void
) {
  const key = hospitalIds.slice().sort((a, b) => a - b).join(',');
  useEffect(() => {
    if (hospitalIds.length === 0) return;
    let unsub: (() => void) | undefined;
    let cancelled = false;

    (async () => {
      try {
        await signalrService.connect();
        if (cancelled) return;
        unsub = signalrService.onAvailabilityUpdated(onUpdate);
        for (const id of hospitalIds) await signalrService.joinHospital(id);
      } catch (e) {
        console.warn('SignalR connect failed', e);
      }
    })();

    return () => {
      cancelled = true;
      if (unsub) unsub();
      hospitalIds.forEach((id) => signalrService.leaveHospital(id).catch(() => undefined));
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [key]);
}
