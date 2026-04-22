import { useEffect, useState } from "react";
import { Formik, Form, Field, ErrorMessage } from "formik";
import * as Yup from "yup";
import phoneMockup from "../../assets/phone-mockup.png";

const RegisterForm = ({ onRegister }) => {
  const [data, setData] = useState([]);

  useEffect(() => {
    fetch("https://provinces.open-api.vn/api/v2/?depth=2")
      .then((res) => res.json())
      .then((data) => setData(data));
  }, []);

  const validationSchema = Yup.object({
    name: Yup.string()
      .required("Tên không được để trống")
      .min(5, "Tên ít nhất phải 5 ký tự"),
    email: Yup.string().email("Email không hợp lệ").required("Bắt buộc"),
    password: Yup.string()
      .min(6, "Ít nhất 6 ký tự")
      .max(16, "Tối đa 16 ký tự")
      .matches(/[a-z]/, "Phải có ít nhất 1 chữ thường")
      .matches(/[A-Z]/, "Phải có ít nhất 1 chữ hoa")
      .matches(/[^a-zA-Z0-9]/, "Phải có ít nhất 1 ký tự đặc biệt")
      .required("Bắt buộc"),
    confirmPassword: Yup.string()
      .oneOf([Yup.ref("password")], "Mật khẩu không khớp")
      .required("Xác nhận mật khẩu"),
    province: Yup.string().required("Chọn tỉnh/thành"),
    ward: Yup.string().required("Chọn quận/huyện"),
    address: Yup.string().required("Nhập địa chỉ"),
    agree: Yup.boolean().oneOf([true], "Bạn phải đồng ý điều khoản"),
  });

  const initialValues = {
    name: "",
    email: "",
    password: "",
    confirmPassword: "",
    province: "",
    ward: "",
    address: "",
    agree: false,
  };

  const handleSubmit = (values) => {
    const provinceObj = data.find((p) => p.codename === values.province);
    const wardObj = provinceObj?.wards?.find((w) => w.codename === values.ward);

    onRegister({
      name: values.name,
      email: values.email,
      password: values.password,
      address: {
        province: provinceObj?.name || "",
        ward: wardObj?.name || "",
        detail: values.address,
      },
    });
  };

  return (
    <div className="min-h-screen flex items-center justify-between bg-green-100 px-6 md:px-16">
      <div className="hidden md:flex w-1/2 justify-center items-center pr-10">
        <img
          src={phoneMockup}
          alt="Garbage Collection"
          className="w-[80%] max-w-md drop-shadow-2xl"
        />
      </div>

      <div className="w-full md:w-1/2 flex justify-start md:pl-10">
        <div className="w-full max-w-2xl bg-white rounded-2xl shadow-xl p-6 md:p-8">
          <h2 className="text-2xl font-bold text-green-700 text-center mb-6">
            Đăng ký tài khoản thu gom rác
          </h2>

          <Formik
            initialValues={initialValues}
            validationSchema={validationSchema}
            onSubmit={handleSubmit}
          >
            {() => (
              <Form className="space-y-5">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="text-sm text-green-700 font-medium">
                      Họ tên
                    </label>
                    <Field className="input" name="name" />
                    <ErrorMessage
                      name="name"
                      className="error"
                      component="div"
                    />
                  </div>

                  <div>
                    <label className="text-sm text-green-700 font-medium">
                      Email
                    </label>
                    <Field className="input" name="email" />
                    <ErrorMessage
                      name="email"
                      className="error"
                      component="div"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="text-sm text-green-700 font-medium">
                      Mật khẩu
                    </label>
                    <Field type="password" className="input" name="password" />
                    <ErrorMessage
                      name="password"
                      className="error"
                      component="div"
                    />
                  </div>

                  <div>
                    <label className="text-sm text-green-700 font-medium">
                      Xác nhận mật khẩu
                    </label>
                    <Field
                      type="password"
                      className="input"
                      name="confirmPassword"
                    />
                    <ErrorMessage
                      name="confirmPassword"
                      className="error"
                      component="div"
                    />
                  </div>
                </div>

                {/* <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <div>
                    <label className="text-sm text-green-700 font-medium">
                      Tỉnh / Thành
                    </label>
                    <Field
                      as="select"
                      name="province"
                      className="input"
                      onChange={(e) => {
                        setFieldValue("province", e.target.value);
                        setFieldValue("ward", "");
                      }}
                    >
                      <option value="">Chọn tỉnh</option>
                      {data.map((p) => (
                        <option key={p.code} value={p.codename}>
                          {p.name}
                        </option>
                      ))}
                    </Field>
                    <ErrorMessage
                      name="province"
                      className="error"
                      component="div"
                    />
                  </div>

                  <div>
                    <label className="text-sm text-green-700 font-medium">
                      Quận / Huyện
                    </label>
                    <Field
                      as="select"
                      name="ward"
                      disabled={!values.province}
                      className="input disabled:bg-gray-100"
                    >
                      <option value="">Chọn quận</option>
                      {data
                        .find((p) => p.codename === values.province)
                        ?.wards?.map((w) => (
                          <option key={w.code} value={w.codename}>
                            {w.name}
                          </option>
                        ))}
                    </Field>
                    <ErrorMessage
                      name="ward"
                      className="error"
                      component="div"
                    />
                  </div>

                  <div className="md:col-span-2">
                    <label className="text-sm text-green-700 font-medium">
                      Số nhà / Đường
                    </label>
                    <Field className="input" name="address" />
                    <ErrorMessage
                      name="address"
                      className="error"
                      component="div"
                    />
                  </div>
                </div> */}

                <div>
                  <div className="flex items-center gap-2">
                    <Field type="checkbox" name="agree" />
                    <label className="text-sm text-gray-700">
                      Tôi đồng ý với quy định thu gom rác
                    </label>
                  </div>
                  <ErrorMessage
                    name="agree"
                    className="error"
                    component="div"
                  />
                </div>

                <button
                  type="submit"
                  className="w-full bg-green-600 hover:bg-green-700 text-white font-semibold py-2 rounded-lg transition"
                >
                  Đăng ký tham gia
                </button>

                <style>{`
                .input {
                  width: 100%;
                  margin-top: 4px;
                  padding: 10px;
                  border: 1px solid #c8e6c9;
                  border-radius: 8px;
                  outline: none;
                }
                .input:focus {
                  border-color: #2e7d32;
                  box-shadow: 0 0 0 2px rgba(46,125,50,0.2);
                }
                .error {
                  font-size: 12px;
                  color: red;
                  margin-top: 4px;
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

export default RegisterForm;
