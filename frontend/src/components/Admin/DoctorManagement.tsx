import { useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { adminApi, hospitalApi } from '../../services/api';
import type { Doctor } from '../../types';

interface Props { hospitalId: number; onChanged?: () => void; }

export function DoctorManagement({ hospitalId, onChanged }: Props) {
  const [docs, setDocs] = useState<Doctor[]>([]);
  const [loading, setLoading] = useState(true);

  const load = async () => {
    setLoading(true);
    try {
      const list = await hospitalApi.doctors(hospitalId);
      setDocs(list);
    } catch (e: any) {
      toast.error(e?.response?.data?.error ?? 'Failed to load doctors');
    } finally { setLoading(false); }
  };

  useEffect(() => { void load(); }, [hospitalId]);

  const toggle = async (d: Doctor) => {
    const prev = d.isAvailable;
    setDocs((curr) => curr.map((x) => (x.id === d.id ? { ...x, isAvailable: !prev } : x)));
    try {
      await adminApi.toggleDoctor(d.id, !prev, null);
      toast.success(`${d.name} → ${!prev ? 'Available' : 'Unavailable'}`);
      onChanged?.();
    } catch (e: any) {
      setDocs((curr) => curr.map((x) => (x.id === d.id ? { ...x, isAvailable: prev } : x)));
      toast.error(e?.response?.data?.error ?? 'Update failed');
    }
  };

  return (
    <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
      <h3 className="font-semibold text-slate-800 mb-3">Doctor Availability</h3>
      {loading ? <p className="text-sm text-slate-500">Loading…</p> : (
        <ul className="divide-y divide-slate-100 max-h-[420px] overflow-auto">
          {docs.map((d) => (
            <li key={d.id} className="flex items-center justify-between py-2">
              <div>
                <div className="font-medium text-slate-800">{d.name}</div>
                <div className="text-xs text-slate-500">{d.specialization} · {d.yearsOfExperience}y · ₹{d.consultationFee}</div>
              </div>
              <button
                onClick={() => toggle(d)}
                className={`relative inline-flex h-6 w-11 items-center rounded-full transition ${d.isAvailable ? 'bg-emerald-500' : 'bg-slate-300'}`}
                aria-label="Toggle availability"
              >
                <span className={`inline-block h-4 w-4 transform rounded-full bg-white transition ${d.isAvailable ? 'translate-x-6' : 'translate-x-1'}`} />
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
