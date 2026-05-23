import { useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { adminApi, hospitalApi } from '../../services/api';
import type { Hospital } from '../../types';

interface Props { hospitalId: number; onChanged?: () => void; }

export function OTStatus({ hospitalId, onChanged }: Props) {
  const [ots, setOts] = useState<Hospital['departments']>([]);
  const [loading, setLoading] = useState(true);

  const load = async () => {
    setLoading(true);
    try {
      const h = await hospitalApi.getById(hospitalId);
      setOts(h.departments.filter((d) => d.type === 'OT'));
    } catch (e: any) {
      toast.error(e?.message ?? 'Failed to load OT');
    } finally { setLoading(false); }
  };

  useEffect(() => { void load(); }, [hospitalId]);

  const toggle = async (id: number, isActive: boolean) => {
    try {
      await adminApi.updateOt(id, isActive);
      toast.success(`OT updated`);
      setOts((curr) => curr.map((x) => (x.id === id ? { ...x, isActive } : x)));
      onChanged?.();
    } catch (e: any) {
      toast.error(e?.response?.data?.error ?? 'Update failed');
    }
  };

  if (loading) return <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm text-sm text-slate-500">Loading OTs…</div>;
  if (ots.length === 0) return <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm text-sm text-slate-500">No OT departments configured.</div>;

  return (
    <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
      <h3 className="font-semibold text-slate-800 mb-3">Operation Theatres</h3>
      <ul className="space-y-2">
        {ots.map((o) => (
          <li key={o.id} className="flex items-center justify-between rounded-md border border-slate-100 px-3 py-2">
            <div>
              <div className="font-medium text-slate-800">{o.name}</div>
              <div className="text-xs text-slate-500">{o.totalBeds} rooms</div>
            </div>
            <select
              value={o.isActive ? 'active' : 'inactive'}
              onChange={(e) => toggle(o.id, e.target.value === 'active')}
              className="rounded border border-slate-300 px-2 py-1 text-sm"
            >
              <option value="active">Active</option>
              <option value="inactive">Inactive</option>
            </select>
          </li>
        ))}
      </ul>
    </div>
  );
}
