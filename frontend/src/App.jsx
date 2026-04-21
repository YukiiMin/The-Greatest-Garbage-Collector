import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import LandingPage from "./pages/landing/LandingPage";
import NotFoundPage from "./pages/NotFoundPage";
import CitizenPage from "./role/citizen/page/CitizenPage";
import AuthPage from "./pages/auth/AuthPage";

const queryClient = new QueryClient();

const App = () => {
  return (
    <>
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<LandingPage />} />

            <Route path="/auth" element={<AuthPage />}>
              <Route path="login" element={<AuthPage />} />
              <Route path="register" element={<AuthPage />} />
            </Route>

            <Route path="/citizen" element={<CitizenPage />}>
              <Route path="report" element={<CitizenPage />} />
              <Route path="report/:reportId" element={<CitizenPage />} />
              <Route path="complaint" element={<CitizenPage />} />
              <Route path="reward" element={<CitizenPage />} />
              <Route path="profile" element={<CitizenPage />} />
            </Route>

            

            <Route path="*" element={<NotFoundPage />} />
          </Routes>
        </BrowserRouter>
      </QueryClientProvider>
    </>
  );
};

export default App;
