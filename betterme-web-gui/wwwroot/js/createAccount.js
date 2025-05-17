console.log('createAccount.js loaded');

(() => {
  'use strict';
  document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('createAccountForm');
    if (!form) return;

    form.addEventListener('submit', async event => {
      //guys the was-validated is like a "check" on the event listener that says aight all data inputs are good
      form.classList.remove('was-validated');

      const txtUsername = form.querySelector('#txtUsername');
      const txtName = form.querySelector('#txtFullName');
      const txtEmail    = form.querySelector('#txtEmail');
      const dtpBirthday    = form.querySelector('#dtpBirthDate');
      const pwdPassword      = form.querySelector('#pwdPassword');
      const pwdConfrimPassword      = form.querySelector('#pwdRepeat');

      txtUsername.setCustomValidity('');
      txtName.setCustomValidity('');
      pwdConfrimPassword.setCustomValidity('');
      pwdPassword.setCustomValidity('');

      const passwordRegex = /^(?=.*[A-Z])(?=.*[!@#$%^&*]).{8,}$/;
      if (!passwordRegex.test(pwdPassword.value)) {
        pwdPassword.setCustomValidity(
          'La contraseña debe tener al menos 8 caracteres, 1 mayúscula y 1 carácter especial'
        );
      }

      if (!form.checkValidity()) {
        form.classList.add('was-validated');
        event.preventDefault();
        return;
      }

      if (pwdPassword.value !== pwdConfrimPassword.value) {
        pwdConfrimPassword.setCustomValidity('Las contraseñas no coinciden');
        form.classList.add('was-validated');
        event.preventDefault();
        return;
      }

      // all checks passed that prevents full-page submit to users service
      event.preventDefault();

      const payload = {
        username: txtUsername.value.trim(),
        name:     txtName.value.trim(),
        email:    txtEmail.value.trim(),
        birthday: dtpBirthday.value,
        password: pwdPassword.value,
        description: '',
        phone: '',
        website: ''
      };

      try {
        const res = await fetch('/users', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(payload)
        });

        if (res.status === 201) {
        alert('¡Cuenta creada! Inicia sesión.');
        window.location.href = '/Login';
        return;
        } else {
          let msg = '';
          try {
            const err = await res.clone().json();
            msg = err.msg || JSON.stringify(err);
          } catch {
            msg = await res.text();
          }
          alert('Error del servidor: ' + (msg || res.status));
        }
      } catch (e) {
        console.error(e);
        alert('No se pudo conectar con el servidor.');
      }
    });
  });
})();
