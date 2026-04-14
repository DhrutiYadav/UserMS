import Axios from "../Api/Axios";
import React from "react";
import { useState, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
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

  // useEffect(() => {
  //   const code = new URLSearchParams(window.location.search).get("code");
  
  //   if (code) {
  //     processGitHubLogin(code);
  //   }
  // }, []);

  useEffect(() => {
    const code = new URLSearchParams(window.location.search).get("code");
  
    if (code && !sessionStorage.getItem("githubCodeUsed")) {
      sessionStorage.setItem("githubCodeUsed", "true");
      processGitHubLogin(code);
    }
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const response = await Axios.post(
        "/api/auth/login",
        JSON.stringify({
          userNameOrEmail,
          password,
        }),
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

  const handleFacebookLogin = () => {
    window.FB.init({
      appId: "1395702662361684",
      cookie: true,
      xfbml: true,
      version: "v19.0",
    });
  
    window.FB.login(
      function (response) {
        if (response.authResponse) {
          processFacebookLogin(response);
        } else {
          alert("Facebook login cancelled");
        }
      },
      { scope: "email,public_profile" }
    );
  };
  
  const processFacebookLogin = async (response) => {
    try {
      console.log("Facebook login success:", response);
  
      const apiResponse = await Axios.post(
        "/api/auth/facebook",
        {
          accessToken: response.authResponse.accessToken,
        }
      );
  
      localStorage.setItem(
        "token",
        apiResponse.data.accessToken
      );
  
      navigate("/users", { replace: true });
    } catch (error) {
      console.log(error);
      alert("Facebook login failed");
    }
  };

  const handleGitHubLogin = () => {
    const clientId = "Ov23li34C62vBwVvvMPN";
  
    const redirectUri = "https://localhost:3000";
  
    window.location.href =
      `https://github.com/login/oauth/authorize?client_id=${clientId}&redirect_uri=${redirectUri}&scope=user:email`;
  };

  const processGitHubLogin = async (code) => {
    try {
      const response = await Axios.post("/api/auth/github", {
        code: code,
      });
  
      console.log("GitHub login success:", response.data);
  
      localStorage.setItem("token", response.data.accessToken);
  
      // IMPORTANT: remove code from URL
      window.history.replaceState({}, document.title, "/login");
  
      // clear flag after success
      sessionStorage.removeItem("githubCodeUsed");
  
      navigate("/users", { replace: true });
    } catch (error) {
      console.log(error);
      console.log("STATUS:", error.response?.status);
      console.log("DATA:", error.response?.data);
  
      // clear old code flag on failure
      sessionStorage.removeItem("githubCodeUsed");
  
      alert("GitHub login failed");
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

        <div className="text-center my-3">
          <p>──────── OR ────────</p>
        </div>

        <div className="mb-3">
          <button
              type="button"
              className="btn btn-primary"
              onClick={handleFacebookLogin}
          >
              Login with Facebook
          </button>
          </div>

          <button onClick={handleGitHubLogin}>
            Login with GitHub
          </button>
        <p>
          Do not Registered?
          <Link to="/register">Register</Link>
        </p>
        
      </div>
    </>
  );
};
export default Login;
