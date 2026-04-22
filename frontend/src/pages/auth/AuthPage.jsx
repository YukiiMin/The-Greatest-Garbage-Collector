import { Leaf } from "lucide-react";
import { useLocation, useNavigate, Link } from "react-router-dom";
import LoginForm from "./LoginForm";
import RegisterForm from "./RegisterForm";

const AuthPage = () => {
  const location = useLocation();
  const navigate = useNavigate();

  const isLoginRoute = location.pathname.includes("/auth/login");
  const isRegisterRoute = location.pathname.includes("/auth/register");

  const handleLogin = (data) => {
    console.log("Login:", data);
  };

  const handleRegister = (data) => {
    console.log("Register:", data);
  };

  return (
    <div className="min-h-screen flex flex-col bg-green-50">
      <header className="bg-green-800 text-white shadow-md">
        <div className="max-w-7xl mx-auto flex items-center justify-between px-4 py-3">
          <button
            className="flex items-center gap-2"
            onClick={() => {
              navigate("/");
            }}
          >
            <Leaf className="text-green-400 w-6 h-6 md:w-7 md:h-7 drop-shadow-sm" />
            <h1 className="text-lg md:text-xl font-bold">Garbage Collection</h1>
          </button>

          <div className="flex items-center gap-3">
            <button
              onClick={() => navigate("/auth/login")}
              className={`px-3 py-1.5 rounded-lg text-sm font-medium transition ${
                isLoginRoute
                  ? "bg-white text-green-800"
                  : "text-white hover:bg-green-600"
              }`}
            >
              Login
            </button>

            <button
              onClick={() => navigate("/auth/register")}
              className={`px-3 py-1.5 rounded-lg text-sm font-medium transition ${
                isRegisterRoute
                  ? "bg-white text-green-800"
                  : "text-white hover:bg-green-600"
              }`}
            >
              Register
            </button>
          </div>
        </div>
      </header>

      <div className="flex-1 flex items-center justify-center px-4 py-6 bg-green-100">
        {isLoginRoute ? (
          <LoginForm onLogin={handleLogin} />
        ) : isRegisterRoute ? (
          <RegisterForm onRegister={handleRegister} />
        ) : (
          <LoginForm onLogin={handleLogin} />
        )}
      </div>
    </div>
  );
};

export default AuthPage;
