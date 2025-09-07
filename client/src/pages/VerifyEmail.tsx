import { useEffect, useState } from "react";
import { useLocation } from "wouter";

export default function VerifyEmail() {
  const [status, setStatus] = useState("Verifying...");
  const [, navigate] = useLocation();

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const token = params.get("token");

    if (!token) {
      setStatus("❌ No token found.");
      return;
    }

    fetch(`http://localhost:5000/api/auth/verify-email`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ token }),
    })
      .then(async (res) => {
        const data = await res.json();

        if (res.ok) {
          setStatus(`✅ ${data.message || "Email verified!"} Redirecting to login...`);
          setTimeout(() => navigate("/"), 2000);
        } else {
          setStatus(`❌ Verification failed: ${data.message || "Unknown error"}`);
        }
      })
      .catch(() => setStatus("❌ Error verifying email."));
  }, [navigate]);

  return <div className="p-6">{status}</div>;
}
