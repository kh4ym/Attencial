/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./**/*.cs",
    "./wwwroot/index.html"
  ],
  darkMode: "class",
  theme: {
    extend: {
      colors: {
        "error": "#ba1a1a",
        "inverse-on-surface": "#f2f0ed",
        "on-surface-variant": "#5a413f",
        "primary-fixed": "#ffdad7",
        "on-tertiary": "#ffffff",
        "surface-variant": "#e4e2df",
        "secondary-fixed": "#e5e2e1",
        "surface-container-lowest": "#ffffff",
        "primary-container": "#d23e41",
        "outline": "#8d706e",
        "secondary-container": "#e5e2e1",
        "on-tertiary-container": "#fcfcff",
        "surface-container-high": "#eae8e5",
        "tertiary": "#006191",
        "on-primary-container": "#fffbff",
        "tertiary-fixed": "#cbe6ff",
        "on-surface": "#1b1c1a",
        "surface-bright": "#fbf9f6",
        "on-primary-fixed-variant": "#910819",
        "secondary": "#5f5e5e",
        "surface": "#fbf9f6",
        "on-secondary": "#ffffff",
        "on-secondary-container": "#656464",
        "on-tertiary-fixed-variant": "#004b71",
        "inverse-primary": "#ffb3af",
        "surface-tint": "#b3272d",
        "inverse-surface": "#30312f",
        "background": "#fbf9f6",
        "surface-dim": "#dbdad7",
        "primary": "#b0252b",
        "tertiary-fixed-dim": "#8fcdff",
        "on-primary-fixed": "#410005",
        "outline-variant": "#e1bebc",
        "primary-fixed-dim": "#ffb3af",
        "error-container": "#ffdad6",
        "on-error-container": "#93000a",
        "on-secondary-fixed-variant": "#474646",
        "on-tertiary-fixed": "#001e30",
        "surface-container-highest": "#e4e2df",
        "on-error": "#ffffff",
        "on-primary": "#ffffff",
        "on-background": "#1b1c1a",
        "surface-container-low": "#f5f3f0",
        "tertiary-container": "#007bb6",
        "surface-container": "#efeeeb",
        "secondary-fixed-dim": "#c8c6c5",
        "on-secondary-fixed": "#1c1b1b",
        "brand-coral": "#f05454"
      },
      borderRadius: {
        "DEFAULT": "0px",
        "lg": "0px",
        "xl": "0px",
        "full": "9999px"
      },
      spacing: {
        "unit": "8px",
        "margin-mobile": "20px",
        "margin-desktop": "64px",
        "max-width": "1440px",
        "gutter": "24px"
      },
      fontFamily: {
        "display-lg": ["Playfair Display", "serif"],
        "body-md": ["Hanken Grotesk", "sans-serif"],
        "headline-md": ["Playfair Display", "serif"],
        "headline-lg": ["Playfair Display", "serif"],
        "body-lg": ["Hanken Grotesk", "sans-serif"],
        "headline-lg-mobile": ["Playfair Display", "serif"],
        "label-caps": ["Hanken Grotesk", "sans-serif"],
        "label-sm": ["Hanken Grotesk", "sans-serif"]
      },
      fontSize: {
        "display-lg": ["80px", { "lineHeight": "1.1", "letterSpacing": "-0.02em", "fontWeight": "700" }],
        "body-md": ["16px", { "lineHeight": "1.6", "fontWeight": "400" }],
        "headline-md": ["32px", { "lineHeight": "1.3", "fontWeight": "600" }],
        "headline-lg": ["48px", { "lineHeight": "1.2", "fontWeight": "700" }],
        "body-lg": ["18px", { "lineHeight": "1.6", "fontWeight": "400" }],
        "headline-lg-mobile": ["32px", { "lineHeight": "1.2", "fontWeight": "700" }],
        "label-caps": ["12px", { "lineHeight": "1", "letterSpacing": "0.15em", "fontWeight": "700" }],
        "label-sm": ["13px", { "lineHeight": "1", "fontWeight": "500" }]
      }
    }
  },
  plugins: [
    require('@tailwindcss/forms'),
    require('@tailwindcss/container-queries')
  ]
};
