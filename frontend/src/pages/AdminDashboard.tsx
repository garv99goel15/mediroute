import { useEffect, useMemo, useState } from 'react';
import { Navigate } from 'react-router-dom';
import { AdminDashboardView } from '../components/Admin/AdminDashboard';
import { useAuthStore } from '../store/hospitalStore';
import { hospitalApi } from '../services/api';

export function AdminDashboardPage() {
  const { user } = useAuthStore();
  const [allHospitals, setAllHospitals] = useState<{ id: number; name: string }[]>([]);
  const [selected, setSelected] = useState<number | null>(null);

  useEffect(() => {
    if (user?.role === 'SuperAdmin') {
      // Load list of hospitals so super-admin can pick one.
      hospitalApi.nearby(28.6139, 77.2090, 100).then((list) =>
        setAllHospitals(list.map((h: any) => ({ id: h.id, name: h.name })))
      );
    }
  }, [user]);

  const targetId = useMemo(() => {
    if (!user) return null;
    if (user.role === 'HospitalAdmin') return user.hospitalId;
    return selected ?? allHospitals[0]?.id ?? null;
  }, [user, selected, allHospitals]);

  if (!user) return <Navigate to="/admin/login" replace />;

  return (
    <div className="mx-auto max-w-7xl px-4 py-6">
      {user.role === 'SuperAdmin' && (
        <div className="mb-4">
          <label className="text-sm font-medium text-slate-700 mr-2">Hospital:</label>
          <select
            value={targetId ?? ''}
            onChange={(e) => setSelected(Number(e.target.value))}
            className="rounded-md border border-slate-300 px-3 py-1.5 text-sm"
          >
            {allHospitals.map((h) => <option key={h.id} value={h.id}>{h.name}</option>)}
          </select>
        </div>
      )}
      {targetId ? (
        <AdminDashboardView hospitalId={targetId} />
      ) : (
        <p className="text-sm text-slate-500">No hospital selected.</p>
      )}
    </div>
  );
}
