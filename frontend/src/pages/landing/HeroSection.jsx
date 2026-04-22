import { useState, useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";
import phoneMockup from "../../assets/phone-mockup.png";

function useScrollAnimation(threshold = 0.1) {
  const ref = useRef(null);
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          setIsVisible(true);
        }
      },
      { threshold },
    );

    if (ref.current) observer.observe(ref.current);

    return () => observer.disconnect();
  }, [threshold]);

  return { ref, isVisible };
}

function FloatingLeaf({ className }) {
  return (
    <svg className={className} viewBox="0 0 40 40" fill="none">
      <path
        d="M20 2C20 2 8 10 8 22C8 30 13 36 20 38C27 36 32 30 32 22C32 10 20 2 20 2Z"
        fill="currentColor"
        opacity="0.15"
      />
      <path d="M20 8V32" stroke="currentColor" opacity="0.2" strokeWidth="1" />
    </svg>
  );
}

export default function HeroSection() {
  const { ref, isVisible } = useScrollAnimation(0.1);
  const navigate = useNavigate();

  return (
    <section
      ref={ref}
      className="relative min-h-screen flex items-center overflow-hidden pt-20 bg-gradient-to-r from-green-400 to-white"
    >
      <FloatingLeaf className="absolute top-[15%] left-[8%] w-12 h-12 text-primary animate-float opacity-40" />
      <FloatingLeaf className="absolute top-[30%] right-[12%] w-16 h-16 text-primary animate-float-delayed opacity-30" />
      <FloatingLeaf className="absolute bottom-[20%] left-[15%] w-10 h-10 text-primary animate-float-slow opacity-25" />
      <FloatingLeaf className="absolute top-[60%] right-[25%] w-8 h-8 text-primary animate-float opacity-20" />

      <div className="absolute top-0 right-0 w-[600px] h-[600px] rounded-full bg-primary/5 blur-3xl -translate-y-1/2 translate-x-1/3" />
      <div className="absolute bottom-0 left-0 w-[400px] h-[400px] rounded-full bg-accent/10 blur-3xl translate-y-1/3 -translate-x-1/4" />

      <div className="container mx-auto px-4 lg:px-8 py-12 lg:py-0">
        <div className="grid lg:grid-cols-2 gap-12 lg:gap-16 items-center">
          <div className="space-y-8">
            <div
              className={`inline-flex items-center gap-2 px-4 py-2 rounded-full bg-primary/10 text-primary text-sm font-medium transition-all duration-700 ${
                isVisible
                  ? "opacity-100 translate-y-0"
                  : "opacity-0 translate-y-4"
              }`}
            >
              🌿 Tuân thủ Quy định Phân loại Rác 2026
            </div>

            <h1
              className={`text-4xl sm:text-5xl lg:text-6xl xl:text-7xl font-extrabold leading-[1.1] tracking-tight text-foreground transition-all duration-700 delay-150 ${
                isVisible
                  ? "opacity-100 translate-y-0"
                  : "opacity-0 translate-y-6"
              }`}
            >
              Biến rác thải thành{" "}
              <span className="text-gradient-primary">điểm thưởng</span> cho
              cộng đồng của bạn
            </h1>

            <p
              className={`text-lg lg:text-xl text-muted-foreground max-w-lg leading-relaxed transition-all duration-700 delay-300 ${
                isVisible
                  ? "opacity-100 translate-y-0"
                  : "opacity-0 translate-y-6"
              }`}
            >
              Chụp ảnh, phân loại, gửi báo cáo trong 30 giây. Doanh nghiệp tái
              chế sẽ đến tận nơi thu gom giúp bạn tiết kiệm thời gian.
            </p>

            <div
              className={`flex flex-wrap gap-4 transition-all duration-700 delay-[450ms] ${
                isVisible
                  ? "opacity-100 translate-y-0"
                  : "opacity-0 translate-y-6"
              }`}
            >
              <button
                className="inline-flex items-center gap-2 px-8 py-4 rounded-full bg-accent text-accent-foreground font-bold text-base shadow-lg hover:scale-[1.03] transition-transform"
                onClick={() => {
                  navigate("/auth");
                }}
              >
                Báo cáo rác ngay →
              </button>

              <a
                href="#process"
                className="inline-flex items-center gap-2 px-8 py-4 rounded-full border-2 border-primary/20 text-primary font-semibold text-base hover:bg-primary/5 transition-colors"
              >
                Xem cách hoạt động ↓
              </a>
            </div>
          </div>

          <div
            className={`relative flex justify-center transition-all duration-1000 delay-500 ${
              isVisible
                ? "opacity-100 translate-y-0"
                : "opacity-0 translate-y-12"
            }`}
          >
            <img
              src={phoneMockup}
              alt="Garbage Collection app"
              className="w-[280px] sm:w-[320px] lg:w-[380px] drop-shadow-2xl"
              width={600}
              height={1024}
            />
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mt-12 lg:mt-16 max-w-3xl mx-auto lg:mx-0"></div>
      </div>
    </section>
  );
}
