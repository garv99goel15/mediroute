import { Component, ReactNode } from 'react';

interface State { hasError: boolean; message?: string; }

export class ErrorBoundary extends Component<{ children: ReactNode }, State> {
  state: State = { hasError: false };

  static getDerivedStateFromError(err: Error): State {
    return { hasError: true, message: err.message };
  }

  componentDidCatch(err: Error) {
    console.error('UI error:', err);
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="m-8 rounded-lg border border-rose-200 bg-rose-50 p-6 text-rose-800">
          <h2 className="font-semibold mb-1">Something went wrong.</h2>
          <p className="text-sm">{this.state.message}</p>
          <button
            onClick={() => this.setState({ hasError: false })}
            className="mt-3 rounded-md bg-rose-600 px-3 py-1.5 text-white text-sm hover:bg-rose-700"
          >Try again</button>
        </div>
      );
    }
    return this.props.children;
  }
}
