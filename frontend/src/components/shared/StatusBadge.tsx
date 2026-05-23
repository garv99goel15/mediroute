interface Props {
  color: 'green' | 'yellow' | 'red' | 'grey';
  label: string;
}

const colors: Record<Props['color'], string> = {
  green: 'bg-emerald-100 text-emerald-700 border-emerald-300',
  yellow: 'bg-amber-100 text-amber-700 border-amber-300',
  red: 'bg-rose-100 text-rose-700 border-rose-300',
  grey: 'bg-slate-100 text-slate-600 border-slate-300',
};

export function StatusBadge({ color, label }: Props) {
  return (
    <span className={`inline-flex items-center gap-1 rounded-full border px-2.5 py-0.5 text-xs font-medium ${colors[color]}`}>
      <span className={`h-2 w-2 rounded-full ${
        color === 'green' ? 'bg-emerald-500'
        : color === 'yellow' ? 'bg-amber-500'
        : color === 'red' ? 'bg-rose-500'
        : 'bg-slate-400'}`} />
      {label}
    </span>
  );
}
