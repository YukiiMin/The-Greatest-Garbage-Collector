import { useState, useEffect, useRef } from "react";
import { X, Check } from "lucide-react";

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

const problems = [
  "Không có giao diện ủng hộ phân loại rác",
  "Không có động lực duy trì thói quen",
  "Thiếu minh bạch trong quy trình thu gom",
];

const solutions = [
  "Giao diện chọn loại rác trực quan, có hướng dẫn",
  "Hệ thống điểm thưởng gamified + bảng xếp hạng",
  "Theo dõi trạng thái từng báo cáo theo thời gian thực",
];

export default function ProblemSolution() {
  const { ref, isVisible } = useScrollAnimation();

  return (
    <section id="problem" ref={ref} className="py-24 lg:py-32 bg-gradient-to-r from-blue-100 to-green-100">
      <div className="container mx-auto px-4 lg:px-8">
        <h2
          className={`text-3xl lg:text-5xl font-extrabold text-center mb-4 text-foreground transition-all duration-700 ${
            isVisible ? "opacity-100 translate-y-0" : "opacity-0 translate-y-8"
          }`}
        >
          Rác thải đô thị — bài toán chưa có lời giải?
        </h2>
        <p
          className={`text-center text-muted-foreground text-lg mb-16 max-w-2xl mx-auto transition-all duration-700 delay-150 ${
            isVisible ? "opacity-100 translate-y-0" : "opacity-0 translate-y-8"
          }`}
        >
          Garbage Collection mang đến giải pháp toàn diện cho mọi vấn đề
        </p>

        <div className="grid md:grid-cols-2 gap-8 max-w-5xl mx-auto">
          <div
            className={`bg-gray-300 rounded-3xl p-8 lg:p-10 transition-all duration-700 delay-200 ${
              isVisible
                ? "opacity-100 translate-y-0"
                : "opacity-0 translate-y-8"
            }`}
          >
            <div className="inline-flex items-center gap-2 px-3 py-1.5 rounded-full bg-destructive/10 text-destructive text-sm font-semibold mb-6">
              Trước đây
            </div>
            <div className="space-y-5">
              {problems.map((p, i) => (
                <div key={i} className="flex items-start gap-3">
                  <div className="mt-0.5 w-6 h-6 rounded-full bg-destructive/10 flex items-center justify-center shrink-0">
                    <X className="w-3.5 h-3.5 text-destructive" />
                  </div>
                  <p className="text-foreground font-medium">{p}</p>
                </div>
              ))}
            </div>
          </div>

          <div
            className={`bg-green-300 rounded-3xl p-8 lg:p-10 transition-all duration-700 delay-300 ${
              isVisible
                ? "opacity-100 translate-y-0"
                : "opacity-0 translate-y-8"
            }`}
          >
            <div className="inline-flex items-center gap-2 px-3 py-1.5 rounded-full bg-primary/10 text-primary text-sm font-semibold mb-6">
              ✨ Với Garabge Collection
            </div>
            <div className="space-y-5">
              {solutions.map((s, i) => (
                <div key={i} className="flex items-start gap-3">
                  <div className="mt-0.5 w-6 h-6 rounded-full bg-primary/10 flex items-center justify-center shrink-0">
                    <Check className="w-3.5 h-3.5 text-primary" />
                  </div>
                  <p className="text-foreground font-medium">{s}</p>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
