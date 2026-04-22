import { useState, useRef, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Leaf } from "lucide-react";

function useScrollAnimation(threshold = 0.1) {
  const ref = useRef(null);
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) setIsVisible(true);
      },
      { threshold },
    );

    if (ref.current) observer.observe(ref.current);

    return () => observer.disconnect();
  }, [threshold]);

  return { ref, isVisible };
}

export default function CTASection() {
  const { ref, isVisible } = useScrollAnimation();
  const navigate = useNavigate();

  return (
    <section
      id="cta"
      ref={ref}
      className="relative py-24 lg:py-32 overflow-hidden bg-green-700"
    >
      <Leaf className="absolute top-10 left-[10%] w-20 h-20 text-primary-foreground/5 animate-float" />
      <Leaf className="absolute bottom-10 right-[15%] w-16 h-16 text-primary-foreground/5 animate-float-delayed" />

      <div className="container mx-auto px-4 lg:px-8 text-center relative z-10">
        <button
          className={`text-3xl lg:text-5xl font-extrabold mb-4 text-primary-foreground transition-all duration-700 ${
            isVisible ? "opacity-100 translate-y-0" : "opacity-0 translate-y-8"
          }`}
          onClick={() => {
            navigate("/auth");
          }}
        >
          Bắt đầu ngay hôm nay — miễn phí
        </button>
        <p
          className={`text-lg mb-10 max-w-xl mx-auto transition-all duration-700 delay-150 ${
            isVisible ? "opacity-100 translate-y-0" : "opacity-0 translate-y-8"
          }`}
          style={{ color: "hsl(140 20% 70%)" }}
        >
          Chỉ cần điện thoại và 30 giây. Rác của bạn sẽ được thu gom ngay trong
          hôm nay.
        </p>
        <div
          className={`transition-all duration-700 delay-300 ${
            isVisible ? "opacity-100 translate-y-0" : "opacity-0 translate-y-8"
          }`}
        >
          <button
            className="inline-flex items-center gap-2 px-10 py-5 rounded-full bg-accent text-accent-foreground font-bold text-lg shadow-xl hover:scale-[1.03] transition-transform bg-yellow-200 hover:bg-yellow-500"
            onClick={() => {
              navigate("/auth");
            }}
          >
            Tạo báo cáo đầu tiên →
          </button>
          <p className="mt-4 text-sm" style={{ color: "hsl(140 15% 55%)" }}>
            Không cần cài app · Mở trên trình duyệt
          </p>
        </div>
      </div>
    </section>
  );
}
