import { useState, useRef, useEffect } from "react";

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

const steps = [
  {
    icon: "📸",
    title: "Chụp ảnh rác",
    desc: "Mở Garbage Collection lên, chụp ảnh đống rác cần thu gom",
    badge: {
      text: "Điền Form",
      color: "bg-green-200 text-green-500",
    },
  },
  {
    icon: "🗂",
    title: "Phân loại",
    desc: "Chọn loại rác: Hữu cơ, Tái chế, Nguy hại hoặc Cồng kềnh.",
    badge: {
      text: "Phân Loại",
      color: "bg-yellow-200 text-yellow-500",
    },
  },
  {
    icon: "📤",
    title: "Gửi báo cáo",
    desc: "Bấm gửi — hệ thống tự động phân về đội thu gom gần nhất trong khu vực bạn.",
    badge: {
      text: "Gửi Báo Cáo",
      color: "bg-blue-200 text-blue-500",
    },
  },
  {
    icon: "🚛",
    title: "Chờ đợi đội thu gom",
    desc: "Đội thu gom sẽ tới và thu gom rác. Bạn nhận thông báo khi rác được dọn sạch.",
    badge: {
      text: "Chờ Đợi Thu Gom",
      color: "bg-purple-200 text-purple-500",
    },
  },
  {
    icon: "🏆",
    title: "Nhận điểm thưởng",
    desc: "Báo cáo hoàn tất = điểm thưởng vào tài khoản. Tích lũy để leo hạng trong khu vực!",
    badge: {
      text: "Nhận thưởng",
      color: "bg-red-200 text-red-500",
    },
  },
];

export default function ProcessSteps() {
  const { ref, isVisible } = useScrollAnimation();
  const [active, setActive] = useState(0);

  return (
    <section id="process" ref={ref} className="py-24 lg:py-32 bg-green-200">
      <div className="container mx-auto px-4 lg:px-8">
        <h2
          className={`text-3xl lg:text-5xl font-extrabold text-center mb-4 text-foreground transition-all duration-700 ${
            isVisible ? "opacity-100 translate-y-0" : "opacity-0 translate-y-8"
          }`}
        >
          Chỉ 5 bước — dễ hơn bạn nghĩ
        </h2>
        <p
          className={`text-center text-muted-foreground text-lg mb-16 transition-all duration-700 delay-100 ${
            isVisible ? "opacity-100 translate-y-0" : "opacity-0 translate-y-8"
          }`}
        >
          Từ chụp ảnh đến nhận thưởng, chỉ trong vài phút
        </p>

        <div className="hidden lg:flex gap-4 max-w-6xl mx-auto">
          {steps.map((step, i) => (
            <button
              key={i}
              onMouseEnter={() => setActive(i)}
              className={`flex-1 text-left p-6 rounded-2xl border-2 transition-all duration-100 cursor-pointer ${
                active === i
                  ? "border-primary bg-card shadow-lg scale-[1.02]"
                  : "border-transparent bg-card/50 hover:bg-card hover:border-border"
              } ${isVisible ? "opacity-100 translate-y-0" : "opacity-0 translate-y-8"} bg-gray-200`}
              style={{ transitionDelay: `${i * 10}ms` }}
            >
              <div className="text-3xl mb-3">{step.icon}</div>
              <div className="text-xs font-bold text-muted-foreground mb-1">
                Bước {i + 1}
              </div>
              <h3 className="font-bold text-foreground mb-2">{step.title}</h3>
              <p className="text-sm text-muted-foreground leading-relaxed">
                {step.desc}
              </p>
              {active === i && step.badge && (
                <div
                  className={`mt-4 inline-flex px-3 py-1.5 rounded-full text-xs font-bold ${step.badge.color} animate-scale-in`}
                >
                  {step.badge.text}
                </div>
              )}
            </button>
          ))}
        </div>

        <div className="lg:hidden space-y-4">
          {steps.map((step, i) => (
            <button
              key={i}
              onMouseEnter={() => setActive(i)}
              className={`w-full text-left flex gap-4 p-5 rounded-2xl border-2 transition-all duration-100 ${
                active === i
                  ? "border-primary bg-card shadow-lg"
                  : "border-transparent bg-card/50"
              } ${isVisible ? "opacity-100 translate-y-0" : "opacity-0 translate-y-6"}`}
              style={{ transitionDelay: `${i * 10}ms` }}
            >
              <div className="text-3xl shrink-0">{step.icon}</div>
              <div>
                <div className="text-xs font-bold text-muted-foreground">
                  Bước {i + 1}
                </div>
                <h3 className="font-bold text-foreground">{step.title}</h3>
                <p className="text-sm text-muted-foreground mt-1">
                  {step.desc}
                </p>
                {active === i && step.badge && (
                  <div
                    className={`mt-3 inline-flex px-3 py-1.5 rounded-full text-xs font-bold ${step.badge.color} animate-scale-in`}
                  >
                    {step.badge.text}
                  </div>
                )}
              </div>
            </button>
          ))}
        </div>
      </div>
    </section>
  );
}
