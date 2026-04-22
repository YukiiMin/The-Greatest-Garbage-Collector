const Button = ({ children, onClick }) => {
  return (
    <button
      onClick={onClick}
      style={{
        width: "100%",
        padding: "10px",
        background: "#2d6cdf",
        color: "white",
        border: "none",
        borderRadius: "6px",
        cursor: "pointer",
      }}
    >
      {children}
    </button>
  );
};

export default Button;
