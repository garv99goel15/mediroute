export function ContactBanner() {
  return (
    <section className="border-t border-slate-200 bg-gradient-to-r from-brand-50 via-white to-emerald-50">
      <div className="mx-auto max-w-6xl px-4 py-6 grid gap-4 md:grid-cols-[1fr_auto] items-center">
        <div>
          <h3 className="text-base font-semibold text-slate-900">
            Run a hospital? List it on MediRoute.
          </h3>
          <p className="text-sm text-slate-600 mt-0.5">
            Reach thousands of patients searching for live bed availability across Delhi NCR.
            We onboard your team in 24 hours — free during the pilot.
          </p>
        </div>
        <div className="flex flex-wrap gap-2 md:justify-end">
          <a
            href="mailto:onboarding@mediroute.in?subject=Hospital%20onboarding%20request"
            className="inline-flex items-center gap-2 rounded-md bg-brand-600 px-4 py-2 text-sm font-medium text-white hover:bg-brand-700"
          >
            <span>✉</span> onboarding@mediroute.in
          </a>
          <a
            href="tel:+919999000111"
            className="inline-flex items-center gap-2 rounded-md border border-brand-600 px-4 py-2 text-sm font-medium text-brand-700 hover:bg-brand-50"
          >
            <span>📞</span> +91 99990 00111
          </a>
        </div>
      </div>
    </section>
  );
}
