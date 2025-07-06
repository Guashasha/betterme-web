
  async function reportPost(postId) {
    const reason = prompt("¿Por qué quieres reportar esta publicación?");
    if (!reason) return;

    const token = document.cookie
      .split("; ")
      .find(row => row.startsWith("accessToken="))
      ?.split("=")[1];

    if (!token) {
      alert("No estás autenticado.");
      return;
    }

    try {
      const res = await fetch("http://localhost:7072/reports", {
        method: "POST",
        mode: "cors",                   
        headers: {
          "Content-Type": "application/json",
          "Authorization": `Bearer ${token}`
        },
        body: JSON.stringify({ postId, reason })
      });
      if (res.ok) {
        alert("Gracias, tu reporte ha sido enviado.");
      } else {
        alert("Error al reportar: " + await res.text());
      }
    } catch (err) {
      console.error(err);
      alert("Error de red, inténtalo de nuevo.");
    }
  }
