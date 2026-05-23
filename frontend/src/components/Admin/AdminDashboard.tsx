import { useCallback, useEffect, useState } from 'react';
import { adminApi } from '../../services/api';
import type { AdminDashboard, AvailabilityUpdateEvent } from '../../types';
import { LoadingSpinner } from '../shared/LoadingSpinner';
import { useSignalR } from '../../hooks/useSignalR';
import { BedManagement } from './BedManagement';
import { DoctorManagement } from './DoctorManagement';
import { OTStatus } from './OTStatus';
import { AmbulancePanel } from './AmbulancePanel';

interface Props { hospitalId: number; }

function Card({ label, value, sub, tone }: { label: string; value: number | string; sub?: string; tone?: 'green' | 'red' | 'blue' | 'amber' }) {
  const tones: Record<string, string> = {
    green: 'border-emerald-200 bg-emerald-50',
    red: 'border-rose-200 bg-rose-50',
    blue: 'border-brand-200 bg-brand-50',
    amber: 'border-amber-200 bg-amber-50',
  };
  return (
    <div className={`rounded-xl border ${tones[tone ?? 'blue']} p-4`}>
      <div className="text-xs uppercase tracking-wide text-slate-500">{label}</div>
      <div className="mt-1 text-2xl font-bold text-slate-900">{value}</div>
      {sub && <div className="text-xs text-slate-500 mt-0.5">{sub}</div>}
    </div>
  );
}

export function AdminDashboardView({ hospitalId }: Props) {
  const [data, setData] = useState<AdminDashboard | null>(null);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const d = await adminApi.dashboard(hospitalId);
      setData(d);
    } finally { setLoading(false); }
  }, [hospitalId]);

  useEffect(() => { void load(); }, [load]);

  useSignalR(hospitalId, (_e: AvailabilityUpdateEvent) => { void load(); });

  if (loading || !data) return <LoadingSpinner label="Loading dashboard…" />;

  return (
    <div className="space-y-6">
      <header className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">{data.hospitalName}</h1>
          <p className="text-sm text-slate-500">Live operations dashboard</p>
        </div>
        <button
          onClick={() => adminApi.snapshot(hospitalId)}
          className="rounded-md border border-slate-300 bg-white px-3 py-1.5 text-sm hover:bg-slate-50"
        >Save snapshot</button>
      </header>

      <section className="grid grid-cols-2 md:grid-cols-4 gap-3">
        <Card label="Total beds" value={data.totalBeds} sub={`${data.occupiedBeds} occupied`} tone="blue" />
        <Card label="Available beds" value={data.availableBeds} tone="green" />
        <Card label="Doctors available" value={`${data.doctorsAvailable}/${data.doctorsTotal}`} tone="amber" />
        <Card label="Ambulances ready" value={`${data.ambulancesAvailable}/${data.ambulancesTotal}`} tone="blue" />
      </section>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <div className="lg:col-span-2 space-y-4">
          <BedManagement hospitalId={hospitalId} onChanged={load} />
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <OTStatus hospitalId={hospitalId} onChanged={load} />
            <AmbulancePanel hospitalId={hospitalId} onChanged={load} />
          </div>
        </div>
        <div className="space-y-4">
          <DoctorManagement hospitalId={hospitalId} onChanged={load} />
          <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
            <h3 className="font-semibold text-slate-800 mb-3">Live activity</h3>
            <ul className="space-y-2 max-h-[300px] overflow-auto">
              {data.recentActivity.length === 0 && <li className="text-sm text-slate-500">No recent activity yet.</li>}
              {data.recentActivity.map((a, i) => (
                <li key={i} className="text-sm border-l-2 border-brand-500 pl-2">
                  <div className="text-slate-800">{a.description}</div>
                  <div className="text-xs text-slate-500">{a.resourceType} · {new Date(a.timestamp).toLocaleTimeString()} {a.updatedBy && `· ${a.updatedBy}`}</div>
                </li>
              ))}
            </ul>
          </div>
        </div>
      </div>
    </div>
  );
}
