import { useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { adminApi, hospitalApi } from '../../services/api';
import type { BedStatus } from '../../types';

interface BedRow {
  id: number;
  bedNumber: string;
  status: BedStatus;
  departmentId: number;
  departmentName: string;
  departmentType: string;
}

interface Props { hospitalId: number; onChanged?: () => void; }

const statuses: BedStatus[] = ['Available', 'Occupied', 'Reserved', 'Maintenance'];

const statusColor: Record<BedStatus, string> = {
  Available: 'bg-emerald-100 text-emerald-700',
  Occupied: 'bg-rose-100 text-rose-700',
  Reserved: 'bg-amber-100 text-amber-700',
  Maintenance: 'bg-slate-200 text-slate-700',
};

export function BedManagement({ hospitalId, onChanged }: Props) {
  const [beds, setBeds] = useState<BedRow[]>([]);
  const [filter, setFilter] = useState<string>('');
  const [loading, setLoading] = useState(true);

  const load = async () => {
    setLoading(true);
    try {
      const h = await hospitalApi.getById(hospitalId);
      const rows: BedRow[] = [];
      h.departments.forEach((d) => {
        (d.beds ?? []).forEach((b) => rows.push({
          id: b.id,
          bedNumber: b.bedNumber,
          status: b.status,
          departmentId: d.id,
          departmentName: d.name,
          departmentType: d.type,
        }));
      });
      setBeds(rows);
    } catch (e: any) {
      toast.error(e?.response?.data?.error ?? 'Failed to load beds');
    } finally { setLoading(false); }
  };

  useEffect(() => { void load(); }, [hospitalId]);

  const updateStatus = async (bed: BedRow, status: BedStatus) => {
    const prev = bed.status;
    setBeds((curr) => curr.map((b) => (b.id === bed.id ? { ...b, status } : b)));
    try {
      await adminApi.updateBed(bed.id, status);
      toast.success(`${bed.bedNumber} → ${status}`);
      onChanged?.();
    } catch (e: any) {
      setBeds((curr) => curr.map((b) => (b.id === bed.id ? { ...b, status: prev } : b)));
      toast.error(e?.response?.data?.error ?? 'Update failed');
    }
  };

  const filtered = filter ? beds.filter((b) => b.departmentType === filter) : beds;

  return (
    <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
      <div className="flex items-center justify-between mb-3">
        <h3 className="font-semibold text-slate-800">
          Bed Management <span className="text-xs text-slate-500">({beds.length} beds)</span>
        </h3>
        <select
          value={filter}
          onChange={(e) => setFilter(e.target.value)}
          className="rounded-md border border-slate-300 px-2 py-1 text-sm"
        >
          <option value="">All departments</option>
          {Array.from(new Set(beds.map((b) => b.departmentType))).map((t) => (
            <option key={t} value={t}>{t}</option>
          ))}
        </select>
      </div>
      {loading ? <p className="text-sm text-slate-500">Loading…</p> : (
        <div className="max-h-[420px] overflow-auto">
          <table className="w-full text-sm">
            <thead className="bg-slate-50 sticky top-0">
              <tr>
                <th className="px-3 py-2 text-left">Bed</th>
                <th className="px-3 py-2 text-left">Dept</th>
                <th className="px-3 py-2 text-left">Current</th>
                <th className="px-3 py-2 text-left">Change to</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((b) => (
                <tr key={b.id} className="border-t border-slate-100">
                  <td className="px-3 py-2 font-medium">{b.bedNumber}</td>
                  <td className="px-3 py-2 text-slate-600">{b.departmentName}</td>
                  <td className="px-3 py-2">
                    <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${statusColor[b.status]}`}>{b.status}</span>
                  </td>
                  <td className="px-3 py-2">
                    <select
                      value={b.status}
                      onChange={(e) => updateStatus(b, e.target.value as BedStatus)}
                      className="rounded border border-slate-300 px-2 py-0.5 text-xs"
                    >
                      {statuses.map((s) => <option key={s} value={s}>{s}</option>)}
                    </select>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
