import React, { useEffect, useState } from 'react';
import { api } from './api';
import { useAuth } from './store';

export function Register({ onCancel }) {
  const [form, setForm] = useState({ firstName: '', lastName: '', email: '', password: '' });
  const [busy, setBusy] = useState(false), [error, setError] = useState('');
  async function submit(event) {
    event.preventDefault(); setBusy(true); setError('');
    try {
      await api('/auth/register', { method: 'POST', body: JSON.stringify(form) });
      const result = await api('/auth/login', { method: 'POST', body: JSON.stringify({ email: form.email, password: form.password }) });
      useAuth.getState().login(result.accessToken, result.user);
    } catch (error) { setError(error.message); } finally { setBusy(false); }
  }
  return <div className="login"><form className="card" onSubmit={submit}><h1>Create your account</h1>
    {Object.keys(form).map(key => <label key={key}>{({ firstName: 'First name', lastName: 'Last name', email: 'Email', password: 'Password' })[key]}
      <input className="field" required type={key === 'password' ? 'password' : key === 'email' ? 'email' : 'text'} minLength={key === 'password' ? 12 : undefined} autoComplete={key === 'password' ? 'new-password' : key === 'email' ? 'email' : 'name'} value={form[key]} onChange={e => setForm({ ...form, [key]: e.target.value })} /></label>)}
    <p className="muted">Use a password of at least 12 characters.</p>{error && <p role="alert" className="error">{error}</p>}
    <button className="btn" disabled={busy}>{busy ? 'Creating account…' : 'Create account'}</button><button className="btn secondary" type="button" onClick={onCancel}>Back to sign in</button>
  </form></div>;
}

export function Plans() {
  const [plans, setPlans] = useState([]), [error, setError] = useState(''), [loading, setLoading] = useState(true), [message, setMessage] = useState(''), [busy, setBusy] = useState(false);
  useEffect(() => { let active = true; api('/plans').then(async plans => Promise.all(plans.map(async plan => ({ ...plan, versions: await api(`/plans/${plan.planId}/versions`) })))).then(result => { if(active) setPlans(result); }).catch(e => { if(active) setError(e.message); }).finally(() => { if(active) setLoading(false); }); return () => { active = false; }; }, []);
  async function subscribe(version) {
    setBusy(true); setError(''); setMessage('');
    try { await api('/subscriptions', { method: 'POST', body: JSON.stringify({ planVersionId: version.planVersionId }) }); setMessage('Subscription created, awaiting payment. Checkout is not yet enabled.'); }
    catch(e) { setError(e.message); } finally { setBusy(false); }
  }
  return <><h1>Challenge plans</h1><p className="muted">Choose from published plan versions. Payments remain unavailable until challenge provisioning is ready.</p>{loading && <p role="status">Loading plans…</p>}{error && <p role="alert" className="error">{error}</p>}{message && <p role="status">{message}</p>}
    {!loading && !error && plans.length === 0 && <div className="card">No plans have been published yet.</div>}
    <div className="grid">{plans.map(plan => <section className="card" key={plan.planId}><h2>{plan.name}</h2><p>{plan.description}</p>{plan.versions.length === 0 && <p>No active versions available.</p>}{plan.versions.map(version => <div key={version.planVersionId}><p>Version {version.versionNumber} · Capital ₹{Number(version.challengeCapital).toLocaleString('en-IN')} · Fee ₹{Number(version.registrationFee).toLocaleString('en-IN')}</p><button disabled={busy} className="btn" onClick={() => subscribe(version)}>Select plan</button></div>)}</section>)}</div></>;
}
