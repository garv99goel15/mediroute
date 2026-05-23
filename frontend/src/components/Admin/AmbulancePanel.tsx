import { useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { adminApi, hospitalApi } from '../../services/api';
import type { Hospital, AmbulanceStatus } from '../../types';

interface Props { hospitalId: number; onChanged?: () => void; }

const statuses: AmbulanceStatus[] = ['Available', 'Dispatched', 'Maintenance'];

export function AmbulancePanel({ hospitalId, onChanged }: Props) {
  const [ambs, setAmbs] = useState<Hospital['ambulances']>([]);
  const [loading, setLoading] = useState(true);

  const load = async () => {
    setLoading(true);
    try {
      const h = await hospitalApi.getById(hospitalId);
      setAmbs(h.ambulances);
    } catch (e: any) {
      toast.error(e?.message ?? 'Failed to load ambulances');
    } finally { setLoading(false); }
  };

  useEffect(() => { void load(); }, [hospitalId]);

  const update = async (id: number, status: AmbulanceStatus) => {
    try {
      await adminApi.updateAmbulance(id, status);
      toast.success(`Ambulance → ${status}`);
      setAmbs((curr) => curr.map((a) => (a.id === id ? { ...a, status } : a)));
      onChanged?.();
    } catch (e: any) {
      toast.error(e?.response?.data?.error ?? 'Update failed');
    }
  };

  if (loading) return <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm text-sm text-slate-500">Loading ambulances…</div>;

  return (
    <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
      <h3 className="font-semibold text-slate-800 mb-3">Ambulances</h3>
      <ul className="divide-y divide-slate-100">
        {ambs.map((a) => (
          <li key={a.id} className="flex items-center justify-between py-2">
            <div>
              <div className="font-medium text-slate-800">{a.vehicleNumber}</div>
              <div className="text-xs text-slate-500">Updated {new Date(a.lastUpdatedAt).toLocaleTimeString()}</div>
            </div>
            <select
              value={a.status}
              onChange={(e) => update(a.id, e.target.value as AmbulanceStatus)}
              className="rounded border border-slate-300 px-2 py-1 text-sm"
            >
              {statuses.map((s) => <option key={s} value={s}>{s}</option>)}
            </select>
          </li>
        ))}
      </ul>
    </div>
  );
}
