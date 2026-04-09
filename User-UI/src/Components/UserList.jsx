import React, { useEffect, useState } from "react";
import Axios from "../Api/Axios";

const UserList = () => {
  const [users, setUsers] = useState([]);

  useEffect(() => {
    getUsers();
  }, []);

  const getUsers = async () => {
    try {
      const response = await Axios.get("/api/auth/usersDisplay");
      setUsers(response.data);
    } catch (error) {
      console.error("Error fetching users:", error);
    }
  };

  return (
    <div className="container mt-5">
      <h2 className="text-center mb-4">User List</h2>

      <div className="table-responsive">
        <table className="table table-bordered table-hover table-striped shadow">
          <thead className="table-dark">
            <tr>
              <th>First Name</th>
              <th>Last Name</th>
              <th>User Name</th>
              <th>Email</th>
              <th>Phone No</th>
              <th>Role</th>
            </tr>
          </thead>

          <tbody>
            {users.length > 0 ? (
              users.map((user, index) => (
                <tr key={index}>
                  <td>{user.firstName}</td>
                  <td>{user.lastName}</td>
                  <td>{user.userName}</td>
                  <td>{user.email}</td>
                  <td>{user.phoneNo}</td>
                  <td>
                    <span className="badge bg-primary">
                      {user.role}
                    </span>
                  </td>
                </tr>
              ))
            ) : (
              <tr>
                <td colSpan="6" className="text-center">
                  No users found
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default UserList;