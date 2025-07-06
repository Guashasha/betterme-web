$(document).ready( () => {
    const form = document.getElementById('verificationRequestForm');

    if (form) {
        form.addEventListener('submit', event => {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            else {
                event.preventDefault();
                sendVerificationRequest();
            }
            form.classList.add('was-validated');
        });
    }
});

function sendVerificationRequest() {
    const form = document.getElementById('verificationRequestForm');
    const formData = new FormData(form);

    $.ajax({
        type: 'POST',
        url: '/api/VerificationRequests',
        data: formData,
        processData: false,
        contentType: false,
        success: (response) => {
            showNotification(response.success ? 'Operación exitosa' : 'Operación fallida', response.success, response.message);
            if (response.redirectPage) {
                window.location.href = response.redirectPage;
            }
        },
        error: () => {
            showNotification('Error en el servidor', false, 'No es posible conectarse al servidor en este momento. Intentélo de nuevo más tarde');
        }
    });
}

function showNotification(title, operationSuccess,  message) {
    const notificationElement = document.getElementById('notification');
    const titleElement = document.getElementById('title');
    const messsageElement = document.getElementById('message');

    const backgroundStyle = operationSuccess? 'bg-success' : 'bg-warning';
    const textStyle = operationSuccess? 'text-white' : 'text-dark';

    notificationElement.classList.add(backgroundStyle, textStyle);
    titleElement.textContent = title;
    messsageElement.textContent = message;

    console.log(notificationElement)
    const toastBootstrap = bootstrap.Toast.getOrCreateInstance(notificationElement)
    toastBootstrap.show()
}
