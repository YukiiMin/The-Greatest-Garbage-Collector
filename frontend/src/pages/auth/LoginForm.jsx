import { Formik, Form, Field, ErrorMessage } from "formik";
import * as Yup from "yup";
import phoneMockup from "../../assets/phone-mockup.png";

const LoginForm = ({ onLogin }) => {
  const validationSchema = Yup.object({
    email: Yup.string().email("Email không hợp lệ").required("Bắt buộc"),
    password: Yup.string().min(6, "Ít nhất 6 ký tự").required("Bắt buộc"),
  });

  const initialValues = {
    email: "",
    password: "",
  };

  const handleSubmit = (values) => {
    onLogin(values);
  };

  return (
    <div className="h-3/4 w-full flex flex-col md:flex-row bg-green-100">
      <div className="hidden md:flex w-1/2 items-center justify-center bg-green-100">
        <img
          src={phoneMockup}
          alt="Garbage Collection"
          className="w-[65%] max-w-md drop-shadow-xl"
        />
      </div>

      <div className="w-full md:w-1/2 flex items-center justify-center px-4">
        <div className="w-full max-w-md bg-white rounded-2xl shadow-lg p-5 md:p-6">
          <h2 className="text-xl font-bold text-green-700 text-center mb-4">
            Đăng nhập
          </h2>

          <Formik
            initialValues={initialValues}
            validationSchema={validationSchema}
            onSubmit={handleSubmit}
          >
            {() => (
              <Form className="space-y-3">
                <div>
                  <label className="text-xs font-medium text-green-700">
                    Email
                  </label>
                  <Field
                    name="email"
                    className="input"
                    placeholder="example@gmail.com"
                  />
                  <ErrorMessage
                    name="email"
                    component="div"
                    className="error"
                  />
                </div>

                <div>
                  <label className="text-xs font-medium text-green-700">
                    Mật khẩu
                  </label>
                  <Field
                    name="password"
                    type="password"
                    className="input"
                    placeholder="••••••••"
                  />
                  <ErrorMessage
                    name="password"
                    component="div"
                    className="error"
                  />
                </div>

                <button
                  type="submit"
                  className="w-full bg-green-600 hover:bg-green-700 text-white font-semibold py-2 rounded-lg transition"
                >
                  Đăng nhập
                </button>

                <style>{`
                  .input {
                    width: 100%;
                    margin-top: 4px;
                    padding: 8px;
                    border: 1px solid #c8e6c9;
                    border-radius: 8px;
                    outline: none;
                    font-size: 14px;
                  }

                  .input:focus {
                    border-color: #2e7d32;
                    box-shadow: 0 0 0 2px rgba(46,125,50,0.15);
                  }

                  .error {
                    font-size: 11px;
                    color: red;
                    margin-top: 3px;
                  }
                `}</style>
              </Form>
            )}
          </Formik>
        </div>
      </div>
    </div>
  );
};

export default LoginForm;
