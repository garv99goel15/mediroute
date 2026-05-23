import { useCallback, useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { hospitalApi } from '../services/api';
import type { Hospital, Availability } from '../types';
import { HospitalDetail } from '../components/Hospital/HospitalDetail';
import { LoadingSpinner } from '../components/shared/LoadingSpinner';
import { useSignalR } from '../hooks/useSignalR';

export function HospitalDetailPage() {
  const { id } = useParams();
  const hospitalId = id ? Number(id) : null;
  const [hospital, setHospital] = useState<Hospital | null>(null);
  const [availability, setAvailability] = useState<Availability | null>(null);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    if (hospitalId == null) return;
    setLoading(true);
    try {
      const [h, a] = await Promise.all([
        hospitalApi.getById(hospitalId),
        hospitalApi.availability(hospitalId),
      ]);
      setHospital(h);
      setAvailability(a);
    } finally { setLoading(false); }
  }, [hospitalId]);

  useEffect(() => { void load(); }, [load]);

  useSignalR(hospitalId, (e) => {
    setAvailability(e.availability);
  });

  if (loading || !hospital || !availability) return <LoadingSpinner label="Loading hospital…" />;

  return (
    <div className="mx-auto max-w-7xl px-4 py-6">
      <HospitalDetail hospital={hospital} availability={availability} />
    </div>
  );
}
