/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./src/**/*.{html,ts}'],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        primary: '#6467f2',
        'background-light': '#f6f6f8',
        'background-dark': '#101122',
        'background-panel': '#0a0a14',
      },
      fontFamily: {
        display: ['"Inter"', 'sans-serif'],
      },
      borderRadius: {
        DEFAULT: '1rem',
        lg: '2rem',
        xl: '3rem',
        full: '9999px',
      },
      keyframes: {
        slideInFromRight: {
          '0%': {
            transform: 'translateX(100%)',
            opacity: '0.5',
          },
          '100%': {
            transform: 'translateX(0)',
            opacity: '1',
          },
        },
        slideInFromBottom: {
          '0%': {
            transform: 'translateY(40px)',
            opacity: '0',
          },
          '100%': {
            transform: 'translateY(0)',
            opacity: '1',
          },
        },
      },
      animation: {
        'slide-in-from-right': 'slideInFromRight 0.4s ease-out forwards',
        'slide-in-from-bottom': 'slideInFromBottom 0.5s ease-out forwards',
      },
    },
  },
  plugins: [require('@tailwindcss/forms'), require('@tailwindcss/typography')],
};
