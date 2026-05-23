import { useHospitalStore } from '../../store/hospitalStore';

const specs = [
  '', 'Cardiology', 'Neurology', 'Orthopedics', 'Pediatrics', 'Oncology',
  'General Medicine', 'Gynecology', 'Dermatology', 'Pulmonology', 'Nephrology',
  'Gastroenterology', 'Urology', 'ENT', 'Ophthalmology', 'Endocrinology',
];

export function FilterPanel() {
  const { service, specialization, setSpecialization } = useHospitalStore();
  if (service !== 'Doctor') return null;
  return (
    <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
      <label className="block text-sm font-medium text-slate-700 mb-2">Specialization</label>
      <select
        value={specialization}
        onChange={(e) => setSpecialization(e.target.value)}
        className="w-full rounded-md border border-slate-300 px-3 py-2 focus:outline-none focus:ring-2 focus:ring-brand-500"
      >
        {specs.map((s) => <option key={s || 'any'} value={s}>{s || 'Any specialization'}</option>)}
      </select>
    </div>
  );
}
