import { Link, NavLink, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../../store/hospitalStore';

export function Navbar() {
  const { user, logout } = useAuthStore();
  const navigate = useNavigate();

  return (
    <nav className="sticky top-0 z-50 bg-white/90 backdrop-blur border-b border-slate-200">
      <div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-3">
        <Link to="/" className="flex items-center gap-2">
          <div className="h-8 w-8 rounded-md bg-brand-600 text-white grid place-items-center font-bold">M</div>
          <span className="font-semibold text-slate-800">MediRoute</span>
          <span className="hidden sm:inline text-xs text-slate-500">Real-time hospital availability</span>
        </Link>
        <div className="flex items-center gap-2 sm:gap-4 text-sm">
          <NavLink to="/" className={({ isActive }) =>
            `px-2 py-1 rounded ${isActive ? 'text-brand-700 font-medium' : 'text-slate-600 hover:text-slate-900'}`
          }>Find hospital</NavLink>
          {user ? (
            <>
              <NavLink to="/admin/dashboard" className={({ isActive }) =>
                `px-2 py-1 rounded ${isActive ? 'text-brand-700 font-medium' : 'text-slate-600 hover:text-slate-900'}`
              }>Admin</NavLink>
              <span className="hidden sm:inline text-slate-500">Hi, {user.username}</span>
              <button
                onClick={() => { logout(); navigate('/'); }}
                className="rounded-md border border-slate-300 px-3 py-1 text-slate-700 hover:bg-slate-50"
              >Log out</button>
            </>
          ) : (
            <NavLink to="/admin/login"
              className="rounded-md bg-brand-600 px-3 py-1.5 text-white hover:bg-brand-700"
            >Admin login</NavLink>
          )}
        </div>
      </div>
    </nav>
  );
}
