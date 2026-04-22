import Header from "./Header";
import HeroSection from "./HeroSection";
import ProblemSolution from "./ProblemSolution";
import ProcessSteps from "./ProcessSteps";
import WasteCategories from "./WasteCategories";
import CTASection from "./CTASection";
import Footer from "./Footer";

const LandingPage = () => {
  return (
    <div className="min-h-screen opacity-95">
      <Header />
      <HeroSection />
      <ProblemSolution />
      <ProcessSteps />
      <WasteCategories />
      <CTASection />
      <Footer />
    </div>
  );
};

export default LandingPage;
