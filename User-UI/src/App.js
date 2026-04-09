import React from "react";
import { Routes, Route } from "react-router-dom";
import Login from "./Components/Login";
import Register from "./Components/Register";
// import Example1 from './Components/Example1';
import UserList from "./Components/UserList";
function App() {
  return (
    <Routes>
      <Route path="/" element={<Login/>}/>
      <Route path="/register" element={<Register/>}/>
      <Route path="/login" element={<Login/>} />
      {/* <Route path="/example" element={<Example1 />} /> */}
      <Route path="/users" element={<UserList />} />
    </Routes>
  );
}

export default App;
