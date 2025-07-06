async function confirmCode() {
  const code  = document.getElementById('txtCode').value.trim();
  const email = document.getElementById('hdnEmail').value;

  if (!code) {
    alert('Por favor ingresa el código.');
    return;
  }

  try {
    const res = await fetch('/api/verify/confirm', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, code })
    });

    if (res.ok) {
      alert('¡Email verificado y cuenta creada! Por favor inicia sesión.');
      window.location.href = '/Login';
    } else {
      const err = await res.json();
      alert(err.msg || 'Código incorrecto o expirado.');
    }
  } catch (e) {
    console.error(e);
    alert('No se pudo conectar al servidor de verificación.');
  }
}