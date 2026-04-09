import React from "react";
import { useState, useEffect } from "react";
import PhoneInput from "react-phone-input-2";
import "react-phone-input-2/lib/style.css";
import { Link, useNavigate } from "react-router-dom";
import Axios from "../Api/Axios";

const Register = () => {
  const navigate = useNavigate();

  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [userName, setUserName] = useState("");
  const [email, setEmail] = useState("");

  const [phone, setPhone] = useState("");
  const [validPhone, setValidPhone] = useState(false);
  const [password, setPassword] = useState("");
  const [validMatch, setValidMatch] = useState(false);
  const [matchPwd, setMatchPwd] = useState("");

  useEffect(() => {
    const match = password === matchPwd;
    setValidMatch(match);
  }, [password, matchPwd]);

  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      await Axios.post(
        "api/auth/register",
        JSON.stringify({
          firstName,
          lastName,
          userName,
          email,
          phoneNo: phone,
          password,
        }),
        {
          headers: { "Content-Type": "application/json" },
          withCredentials: true,
        }
      );

      setPassword("");
      setMatchPwd("");
      navigate("/login", { replace: true });
    } catch (err) {
      console.log(err);
    }
  };

  return (
    <div className="uiBox">
      <h1>Registration</h1>
      <form className="was-validated" noValidate onSubmit={handleSubmit}>
        <div className="mb-3">
          <label htmlFor="firstName" className="form-label">
            First Name
          </label>
          <input
            type="text"
            className="form-control"
            id="firstName"
            value={firstName}
            placeholder="Enter your name"
            onChange={(e) => setFirstName(e.target.value)}
            required
          />
          <div className="invalid-feedback">Please enter your first name.</div>
        </div>
        <div className="mb-3">
          <label htmlFor="lastName" className="form-label">
            Last Name
          </label>
          <input
            type="text"
            className="form-control"
            id="lastName"
            value={lastName}
            placeholder="Enter your name"
            onChange={(e) => setLastName(e.target.value)}
            required
          />
          <div className="invalid-feedback">Please enter your User name.</div>
        </div>
        <div className="mb-3">
          <label htmlFor="userName" className="form-label">
            User Name
          </label>
          <input
            type="text"
            className="form-control"
            id="userName"
            value={userName}
            placeholder="Enter your user name"
            onChange={(e) => setUserName(e.target.value)}
            required
          />
          <div className="invalid-feedback">Please enter your Last name.</div>
        </div>
        <div className="mb-3">
          <label htmlFor="email" className="form-label">
            Email
          </label>
          <input
            type="email"
            className="form-control"
            id="email"
            value={email}
            placeholder="Enter your name"
            onChange={(e) => setEmail(e.target.value)}
            required
          />
          <div className="invalid-feedback">Please enter your User name.</div>
        </div>
        <div>
          <label htmlFor="phoneNo" className="form-label">
            Mobile Number
          </label>

          <PhoneInput
            id="phoneNo"
            placeholder="Enter your Mobile number"
            country={"in"}
            value={phone}
            onChange={(phone) => {
              setPhone(phone);

              // 🔥 FIX LOGIC (ignore +91)
              const onlyNumber = phone.slice(2); // remove country code (91)

              setValidPhone(
                onlyNumber.length !== 10 // must be exactly 10 digits
              );
            }}
            inputClass={
              phone.length <= 2
                ? "form-control" // only +91 → normal
                : validPhone
                ? "form-control is-invalid"
                : "form-control"
            }
            inputStyle={{ width: "100%" }}
          />

          {validPhone && phone.length > 2 && (
            <div style={{ color: "red" }}>Please enter valid mobile number</div>
          )}

          {/* <p>Entered: +{phone}</p> */}
        </div>

        <div className="mb-3">
          <label htmlFor="password" className="form-label">
            Password
          </label>
          <input
            type="password"
            className="form-control"
            value={password}
            id="password"
            onChange={(e) => setPassword(e.target.value)}
            placeholder="Enter Password"
            required
          />
          <div className="invalid-feedback">Please enterPassword.</div>
        </div>

        <div className="mb-3">
          <label htmlFor="ConfirmPwd" className="form-label">
            Confirm Password
          </label>
          <input
            type="password"
            className="form-control"
            id="ConfirmPwd"
            onChange={(e) => setMatchPwd(e.target.value)}
            placeholder="Enter Password"
            required
          />
          <div className={validMatch && matchPwd ? "invalid-feedback" : ""}>
            Password do not match.
          </div>
        </div>

        <div className="mb-3">
          <button className="btn btn-primary" type="submit">
            Submit form
          </button>
        </div>
      </form>
      <p>
        Already Registered?
        <Link to="/login">Login</Link>
      </p>
    </div>
  );
};

export default Register;
