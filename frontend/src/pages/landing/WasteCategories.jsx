import { useState, useRef, useEffect } from "react";

function useScrollAnimation(threshold = 0.1) {
  const ref = useRef(null);
  const [isVisible, setIsVisible] = useState(null);

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

const categories = [
  {
    icon: "♻️",
    title: "Rác tái chế",
    items: "Giấy, nhựa, kim loại, thủy tinh",
    tip: "Rửa sạch trước khi bỏ vào túi",
    color: "hover:bg-blue-200",
    borderColor: "hover:border-blue-500",
    glowColor: "hover:shadow-[0_8px_30px_-8px_hsl(210_80%_55%/0.3)]",
  },
  {
    icon: "🌿",
    title: "Rác hữu cơ",
    items: "Thực phẩm thừa, lá cây, vỏ trái cây",
    tip: "Bỏ chung tất cả vào 1 bao lớn",
    color: "hover:bg-green-300",
    borderColor: "hover:border-green-500",
    glowColor: "hover:shadow-[0_8px_30px_-8px_hsl(152_55%_23%/0.3)]",
  },
  {
    icon: "⚠️",
    title: "Rác nguy hại",
    items: "Pin, bóng đèn, hóa chất, thuốc cũ",
    tip: "Đừng bỏ chung với rác thông thường!",
    color: "hover:bg-yellow-200",
    borderColor: "hover:border-yellow-500",
    glowColor: "hover:shadow-[0_8px_30px_-8px_hsl(0_72%_51%/0.3)]",
  },
  {
    icon: "📦",
    title: "Rác cồng kềnh",
    items: "Đồ điện tử, nội thất, thiết bị lớn",
    tip: "Cố gắng thu nhỏ thể tích",
    color: "hover:bg-purple-200",
    borderColor: "hover:border-purple-500",
    glowColor: "hover:shadow-[0_8px_30px_-8px_hsl(270_50%_50%/0.3)]",
  },
];

export default function WasteCategories() {
  const { ref, isVisible } = useScrollAnimation();
  const [showTip, setShowTip] = useState(false);

  return (
    <section id="categories" ref={ref} className="py-24 lg:py-32 bg-green-100">
      <div className="container mx-auto px-4 lg:px-8">
        <h2
          className={`text-3xl lg:text-5xl font-extrabold text-center mb-4 text-foreground transition-all duration-700 ${
            isVisible ? "opacity-100 translate-y-0" : "opacity-0 translate-y-8"
          }`}
        >
          Phân loại đúng — nhận thêm điểm
        </h2>
        <p
          className={`text-center text-muted-foreground text-lg mb-16 max-w-2xl mx-auto transition-all duration-700 delay-100 ${
            isVisible ? "opacity-100 translate-y-0" : "opacity-0 translate-y-8"
          }`}
        >
          Mỗi loại rác có giá trị tái chế khác nhau.
          <br />
          Phân loại chính xác sẽ được nhiều điểm thưởng cao hơn.
        </p>

        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 lg:gap-6 max-w-6xl mx-auto">
          {categories.map((cat, i) => (
            <div
              key={i}
              className={`bg-gray-100 group rounded-3xl p-6 lg:p-8 ${cat.color} border-2 border-transparent ${cat.borderColor} ${cat.glowColor} hover-lift cursor-default transition-all duration-700 ${
                isVisible
                  ? "opacity-100 translate-y-0"
                  : "opacity-0 translate-y-8"
              }`}
              style={{ transitionDelay: `${i * 10}ms` }}
              onMouseEnter={() => {
                setShowTip(i);
              }}
              onMouseLeave={() => {
                setShowTip(null);
              }}
            >
              <div></div>
              <div className="text-5xl lg:text-6xl mb-4">{cat.icon}</div>
              <h3 className="font-bold text-lg text-foreground mb-2">
                {cat.title}
              </h3>
              <p className="text-sm text-muted-foreground mb-4">{cat.items}</p>
              {showTip === i ? (
                <p className="text-xs text-muted-foreground italic">
                  💡 {cat.tip}
                </p>
              ) : (
                ""
              )}
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
