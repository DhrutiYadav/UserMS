import Axios from "../Api/Axios";
import React from "react";
import { useState, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
// import Example1 from "./Example1";
// import UserList from "./Components/UserList";
const Login = () => {
  const navigate = useNavigate();
  const [userNameOrEmail, setUserNameOrEmail] = useState("");
  const [password, setPassword] = useState("");
  const [isValid, setIsValid] = useState(false);

  useEffect(() => {
    if (userNameOrEmail.trim() !== "" && password.trim() !== "") {
      setIsValid(true);
    } else {
      setIsValid(false);
    }
  }, [userNameOrEmail, password]);
  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const response = await Axios.post(
        "/api/auth/login",
        JSON.stringify({
          userNameOrEmail,
          password,
        }),
        // {
        //   headers: { "Content-Type": "application/json" },
        //   withCredentials: true,
        // }
      );
      // save JWT token
      localStorage.setItem("token", response.data.accessToken);
      setUserNameOrEmail("");
      setPassword("");
      navigate("/users", { replace: true });
    } catch (err) {
      console.log(err);
      alert("Invalid username or password");
    }
  };
  return (
    <>
      <div className="uiBox">
        <h1>Login</h1>
        <form className="was-validated" noValidate onSubmit={handleSubmit}>
          <div className="mb-3">
            <label htmlFor="userNameOrEmail" className="form-label">
              User Name Or Email Id
            </label>
            <input
              type="text"
              className="form-control"
              id="userNameOrEmail"
              value={userNameOrEmail}
              onChange={(e) => setUserNameOrEmail(e.target.value)}
              placeholder="Enter your name"
              required
            />
            <div className="invalid-feedback">
              Please enter your Username or EmailId.
            </div>
          </div>

          <div className="mb-3">
            <label htmlFor="password" className="form-label">
              Password
            </label>
            <input
              type="password"
              className="form-control"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Enter Password"
              required
            />
            <div className="invalid-feedback">Please enter Password.</div>
          </div>

          <div className="mb-3">
            <button
              className="btn btn-primary"
              type="submit"
              disabled={!isValid}
            >
              Submit form
            </button>
          </div>
        </form>
        <p>
          Do not Registered?
          <Link to="/register">Register</Link>
        </p>
      </div>
    </>
  );
};
export default Login;
