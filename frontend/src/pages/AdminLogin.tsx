import { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { authApi } from '../services/api';
import { useAuthStore } from '../store/hospitalStore';

export function AdminLogin() {
  const [username, setUsername] = useState('superadmin');
  const [password, setPassword] = useState('Admin@123');
  const [loading, setLoading] = useState(false);
  const { setAuth } = useAuthStore();
  const navigate = useNavigate();

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const r = await authApi.login(username, password);
      setAuth(r);
      toast.success(`Welcome ${r.username}`);
      navigate('/admin/dashboard');
    } catch (e: any) {
      toast.error(e?.response?.data?.error ?? 'Login failed');
    } finally { setLoading(false); }
  };

  const fill = (u: string, p: string) => { setUsername(u); setPassword(p); };

  return (
    <div className="mx-auto max-w-2xl px-4 py-10">
      <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
        <h1 className="text-xl font-bold text-slate-900 mb-1">Sign in</h1>
        <p className="text-sm text-slate-500 mb-5">MediRoute has three access levels. Pick a role to auto-fill credentials.</p>

        <div className="grid gap-3 md:grid-cols-3 mb-6">
          <button
            type="button" onClick={() => fill('superadmin', 'Admin@123')}
            className="text-left rounded-lg border border-slate-200 p-3 hover:border-brand-500 hover:bg-brand-50"
          >
            <div className="text-xs font-semibold uppercase tracking-wide text-brand-700">1 · Super Admin</div>
            <div className="mt-1 text-sm font-medium text-slate-900">Platform owner</div>
            <div className="mt-1 text-xs text-slate-500">Manages every hospital, broadcasts updates.</div>
            <div className="mt-2 text-xs"><code>superadmin</code> / <code>Admin@123</code></div>
          </button>
          <button
            type="button" onClick={() => fill('admin1', 'Admin@123')}
            className="text-left rounded-lg border border-slate-200 p-3 hover:border-brand-500 hover:bg-brand-50"
          >
            <div className="text-xs font-semibold uppercase tracking-wide text-emerald-700">2 · Hospital Admin</div>
            <div className="mt-1 text-sm font-medium text-slate-900">Edit own services</div>
            <div className="mt-1 text-xs text-slate-500">Bed status, doctor &amp; ambulance availability for one hospital.</div>
            <div className="mt-2 text-xs"><code>admin1</code>…<code>admin15</code> / <code>Admin@123</code></div>
          </button>
          <div className="rounded-lg border border-dashed border-slate-300 p-3 bg-slate-50">
            <div className="text-xs font-semibold uppercase tracking-wide text-slate-600">3 · Patient / Public</div>
            <div className="mt-1 text-sm font-medium text-slate-900">No login required</div>
            <div className="mt-1 text-xs text-slate-500">Uses your location to find the nearest hospital with live availability.</div>
            <a href="/" className="mt-2 inline-block text-xs font-medium text-brand-700 hover:underline">Go to search →</a>
          </div>
        </div>

        <form onSubmit={submit} className="space-y-3">
          <div>
            <label className="block text-sm font-medium text-slate-700">Username</label>
            <input
              value={username} onChange={(e) => setUsername(e.target.value)}
              className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 focus:outline-none focus:ring-2 focus:ring-brand-500"
              autoComplete="username"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-700">Password</label>
            <input
              type="password" value={password} onChange={(e) => setPassword(e.target.value)}
              className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 focus:outline-none focus:ring-2 focus:ring-brand-500"
              autoComplete="current-password"
            />
          </div>
          <button
            type="submit" disabled={loading}
            className="w-full rounded-md bg-brand-600 px-4 py-2 text-white font-medium hover:bg-brand-700 disabled:opacity-60"
          >{loading ? 'Signing in…' : 'Sign in'}</button>
        </form>
      </div>
    </div>
  );
}
