const NotFoundPage = () => {
  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-100">
      <div className="text-center">
        <h1 className="mb-4 text-4xl font-bold">404</h1>
        <p className="mb-4 text-xl text-gray-500">Oops! Page not found</p>
        <a href="/" className="text-blue-600 underline hover:text-blue-500">
          Return to Home
        </a>
      </div>
    </div>
  );
};

export default NotFoundPage;
