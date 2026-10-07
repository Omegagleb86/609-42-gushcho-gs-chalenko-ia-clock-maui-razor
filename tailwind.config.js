module.exports = {
  content: ["./Components/**/*.razor", "./wwwroot/index.html"],
  theme: {
    extend: {
      colors: { ink: "#22232c", panel: "#2b2d38", rail: "#1c1d25", muted: "#a3a5b8" },
      fontFamily: { sans: ["Open Sans", "system-ui", "sans-serif"] }
    }
  },
  plugins: []
};
