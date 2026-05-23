import { useEffect } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { Navbar } from './components/shared/Navbar';
import { ErrorBoundary } from './components/shared/ErrorBoundary';
import { ContactBanner } from './components/shared/ContactBanner';
import { PatientSearch } from './pages/PatientSearch';
import { HospitalDetailPage } from './pages/HospitalDetail';
import { AdminLogin } from './pages/AdminLogin';
import { AdminDashboardPage } from './pages/AdminDashboard';
import { useAuthStore } from './store/hospitalStore';

function RequireAuth({ children }: { children: JSX.Element }) {
  const { user } = useAuthStore();
  if (!user) return <Navigate to="/admin/login" replace />;
  return children;
}

export default function App() {
  const hydrate = useAuthStore((s) => s.hydrate);
  useEffect(() => { hydrate(); }, [hydrate]);

  return (
    <div className="min-h-full flex flex-col">
      <Navbar />
      <main className="flex-1">
        <ErrorBoundary>
          <Routes>
            <Route path="/" element={<PatientSearch />} />
            <Route path="/hospital/:id" element={<HospitalDetailPage />} />
            <Route path="/admin/login" element={<AdminLogin />} />
            <Route path="/admin/dashboard" element={<RequireAuth><AdminDashboardPage /></RequireAuth>} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </ErrorBoundary>
      </main>
      <ContactBanner />
      <footer className="border-t border-slate-200 bg-white py-3 text-center text-xs text-slate-500">
        © {new Date().getFullYear()} MediRoute · Built by Garv Goel
      </footer>
    </div>
  );
}
