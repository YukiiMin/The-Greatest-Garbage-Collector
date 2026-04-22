import { useState, useEffect } from "react";
import { Leaf, Menu, X } from "lucide-react";
import { useNavigate } from "react-router-dom";

const navItems = [
  { label: "Giới thiệu", href: "#problem" },
  { label: "Quy trình", href: "#process" },
  { label: "Phân loại rác", href: "#categories" },
];

export default function Header() {
  const [scrolled, setScrolled] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    const handleScroll = () => {
      setScrolled(window.scrollY > 80);
    };

    window.addEventListener("scroll", handleScroll);

    return () => {
      window.removeEventListener("scroll", handleScroll);
    };
  }, []);

  return (
    <header
      className={
        "fixed top-0 left-0 right-0 z-50 transition-all duration-300 " +
        (scrolled
          ? "shadow-md backdrop-blur-lg bg-gradient-to-r from-green-300 to-white"
          : "bg-gradient-to-r from-green-300 to-white")
      }
    >
      <div className="flex items-center justify-between py-4 px-4 lg:px-8">
        <a href="" className="flex items-center gap-2">
          <div className="w-9 h-9 bg-green-500 flex items-center justify-center rounded-xl">
            <Leaf className="w-5 h-5 text-white" />
          </div>
          <span className="font-bold text-xl">Garbage Collection</span>
        </a>

        <nav className="hidden md:flex gap-8">
          {navItems.map((item) => (
            <a
              key={item.href}
              href={item.href}
              className="text-sm hover:text-green-500"
            >
              {item.label}
            </a>
          ))}
        </nav>

        <div className="hidden md:block">
          <button
            className="px-6 py-2 bg-green-500 text-white rounded-full"
            onClick={() => {
              navigate("/auth");
            }}
          >
            Tham gia ngay →
          </button>
        </div>

        <button className="md:hidden" onClick={() => setMenuOpen(!menuOpen)}>
          {menuOpen ? <X /> : <Menu />}
        </button>
      </div>

      {menuOpen && (
        <div className="md:hidden border-t p-4">
          {navItems.map((item) => (
            <a
              key={item.href}
              href={item.href}
              className="block py-2"
              onClick={() => setMenuOpen(false)}
            >
              {item.label}
            </a>
          ))}

          <button
            className="block mt-4 text-center bg-green-500 text-white py-3 rounded-full"
            onClick={() => {
              setMenuOpen(false);
              navigate("/auth");
            }}
          >
            Tham gia ngay →
          </button>
        </div>
      )}
    </header>
  );
}
