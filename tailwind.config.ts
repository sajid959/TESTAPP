import type { Config } from "tailwindcss";

export default {
  darkMode: ["class"],
  content: ["./client/index.html", "./client/src/**/*.{js,jsx,ts,tsx}"],
  theme: {
    extend: {
      borderRadius: {
        lg: "var(--radius)",
        md: "calc(var(--radius) - 2px)",
        sm: "calc(var(--radius) - 4px)",
      },
      colors: {
        background: "var(--background)",
        foreground: "var(--foreground)",
        card: {
          DEFAULT: "var(--card)",
          foreground: "var(--card-foreground)",
        },
        popover: {
          DEFAULT: "var(--popover)",
          foreground: "var(--popover-foreground)",
        },
        primary: {
          DEFAULT: "var(--primary)",
          foreground: "var(--primary-foreground)",
        },
        secondary: {
          DEFAULT: "var(--secondary)",
          foreground: "var(--secondary-foreground)",
        },
        muted: {
          DEFAULT: "var(--muted)",
          foreground: "var(--muted-foreground)",
        },
        accent: {
          DEFAULT: "var(--accent)",
          foreground: "var(--accent-foreground)",
        },
        destructive: {
          DEFAULT: "var(--destructive)",
          foreground: "var(--destructive-foreground)",
        },
        border: "var(--border)",
        input: "var(--input)",
        ring: "var(--ring)",
        chart: {
          "1": "var(--chart-1)",
          "2": "var(--chart-2)",
          "3": "var(--chart-3)",
          "4": "var(--chart-4)",
          "5": "var(--chart-5)",
        },
        sidebar: {
          DEFAULT: "var(--sidebar)",
          foreground: "var(--sidebar-foreground)",
          primary: "var(--sidebar-primary)",
          "primary-foreground": "var(--sidebar-primary-foreground)",
          accent: "var(--sidebar-accent)",
          "accent-foreground": "var(--sidebar-accent-foreground)",
          border: "var(--sidebar-border)",
          ring: "var(--sidebar-ring)",
        },
        brand: {
          "50": "var(--brand-50)",
          "100": "var(--brand-100)",
          "200": "var(--brand-200)",
          "300": "var(--brand-300)",
          "400": "var(--brand-400)",
          "500": "var(--brand-500)",
          "600": "var(--brand-600)",
          "700": "var(--brand-700)",
          "800": "var(--brand-800)",
          "900": "var(--brand-900)",
        },
        success: {
          "50": "var(--success-50)",
          "100": "var(--success-100)",
          "200": "var(--success-200)",
          "300": "var(--success-300)",
          "400": "var(--success-400)",
          "500": "var(--success-500)",
          "600": "var(--success-600)",
          "700": "var(--success-700)",
          "800": "var(--success-800)",
          "900": "var(--success-900)",
        },
        warning: {
          "50": "var(--warning-50)",
          "100": "var(--warning-100)",
          "200": "var(--warning-200)",
          "300": "var(--warning-300)",
          "400": "var(--warning-400)",
          "500": "var(--warning-500)",
          "600": "var(--warning-600)",
          "700": "var(--warning-700)",
          "800": "var(--warning-800)",
          "900": "var(--warning-900)",
        },
        error: {
          "50": "var(--error-50)",
          "100": "var(--error-100)",
          "200": "var(--error-200)",
          "300": "var(--error-300)",
          "400": "var(--error-400)",
          "500": "var(--error-500)",
          "600": "var(--error-600)",
          "700": "var(--error-700)",
          "800": "var(--error-800)",
          "900": "var(--error-900)",
        },
      },
      fontFamily: {
        sans: ["var(--font-sans)", "Inter", "ui-sans-serif", "system-ui"],
        serif: ["var(--font-serif)", "ui-serif", "Georgia"],
        mono: ["var(--font-mono)", "JetBrains Mono", "ui-monospace"],
      },
      keyframes: {
        "accordion-down": {
          from: {
            height: "0",
          },
          to: {
            height: "var(--radix-accordion-content-height)",
          },
        },
        "accordion-up": {
          from: {
            height: "var(--radix-accordion-content-height)",
          },
          to: {
            height: "0",
          },
        },
        "fade-in": {
          from: {
            opacity: "0",
          },
          to: {
            opacity: "1",
          },
        },
        "slide-up": {
          from: {
            opacity: "0",
            transform: "translateY(20px)",
          },
          to: {
            opacity: "1",
            transform: "translateY(0)",
          },
        },
      },
      animation: {
        "accordion-down": "accordion-down 0.2s ease-out",
        "accordion-up": "accordion-up 0.2s ease-out",
        "fade-in": "fade-in 0.5s ease-in-out",
        "slide-up": "slide-up 0.3s ease-out",
      },
    },
  },
  plugins: [require("tailwindcss-animate"), require("@tailwindcss/typography")],
} satisfies Config;
