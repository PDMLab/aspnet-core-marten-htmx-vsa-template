module.exports = {
  content: [
    "**/*.{cs,cshtml,html}",
  ],
  safelist: [
    'field-validation-error',
    'input-validation-error',
    'validation-summary-errors'
  ],
  plugins: [
    // eslint-disable-next-line @typescript-eslint/no-var-requires
    require("@tailwindcss/forms")({
      strategy: "class",
    }),
    require("tailwindcss-debug-screens"),
  ],
  theme: {
    debugScreens: {
      position: ['bottom', 'right']
    },
    extend: {
      colors: {
        turquoise: {
          50: '#ecf9fe',
          100: '#d4f2fd',
          200: '#a8e5fb',
          300: '#7dd8f9',
          400: '#67d2f8',
          500: '#51cbf7',
          600: '#47b2d9',
          700: '#3d99ba',
          800: '#29667c',
          900: '#15333e'
        },
      },
    },
  },
  variants: {
    extend: {
      opacity: ["disabled"],
    },
  },
};
