import React, { useEffect, useState } from 'react';
import { usersApi } from '../../services/api';
import { useAuth } from '../../context/AuthContext';
import { User } from '../../types';
import './Users.css';

const Users: React.FC = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState('all');
  const { isAdmin } = useAuth();

  useEffect(() => {
    loadUsers();
  }, []);

  const loadUsers = async () => {
    try {
      const response = await usersApi.getAll();
      setUsers(response.data);
    } catch (error) {
      console.error('Failed to load users:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this user?')) {
      try {
        await usersApi.delete(id);
        loadUsers();
      } catch (error: any) {
        alert(error.response?.data?.message || 'Failed to delete user');
      }
    }
  };

  const handleToggleActive = async (user: User) => {
    try {
      await usersApi.update(user.userId, { isActive: !user.isActive });
      loadUsers();
    } catch (error: any) {
      alert(error.response?.data?.message || 'Failed to update user');
    }
  };

  const filteredUsers = users.filter((user) => {
    if (filter === 'all') return true;
    return user.role.toLowerCase() === filter;
  });

  if (loading) {
    return <div className="loading">Loading users...</div>;
  }

  return (
    <div className="users-page">
      <div className="page-header">
        <h1>👥 Users Management</h1>
        <div className="filter-buttons">
          <button
            className={`filter-btn ${filter === 'all' ? 'active' : ''}`}
            onClick={() => setFilter('all')}
          >
            All ({users.length})
          </button>
          <button
            className={`filter-btn ${filter === 'admin' ? 'active' : ''}`}
            onClick={() => setFilter('admin')}
          >
            Admins ({users.filter((u) => u.role === 'Admin').length})
          </button>
          <button
            className={`filter-btn ${filter === 'teacher' ? 'active' : ''}`}
            onClick={() => setFilter('teacher')}
          >
            Teachers ({users.filter((u) => u.role === 'Teacher').length})
          </button>
          <button
            className={`filter-btn ${filter === 'student' ? 'active' : ''}`}
            onClick={() => setFilter('student')}
          >
            Students ({users.filter((u) => u.role === 'Student').length})
          </button>
        </div>
      </div>

      <div className="users-list">
        <table>
          <thead>
            <tr>
              <th>User</th>
              <th>Email</th>
              <th>Role</th>
              <th>Phone</th>
              <th>Status</th>
              <th>Joined</th>
              {isAdmin && <th>Actions</th>}
            </tr>
          </thead>
          <tbody>
            {filteredUsers.map((user) => (
              <tr key={user.userId}>
                <td>
                  <div className="user-info">
                    <div className="avatar">
                      {user.firstName[0]}{user.lastName[0]}
                    </div>
                    <span>{user.firstName} {user.lastName}</span>
                  </div>
                </td>
                <td>{user.email}</td>
                <td>
                  <span className={`role-badge ${user.role.toLowerCase()}`}>
                    {user.role}
                  </span>
                </td>
                <td>{user.phone || '-'}</td>
                <td>
                  <span className={`status ${user.isActive ? 'active' : 'inactive'}`}>
                    {user.isActive ? 'Active' : 'Inactive'}
                  </span>
                </td>
                <td>{new Date(user.createdAt).toLocaleDateString()}</td>
                {isAdmin && (
                  <td>
                    <div className="action-buttons">
                      <button
                        className={`btn-toggle ${user.isActive ? 'deactivate' : 'activate'}`}
                        onClick={() => handleToggleActive(user)}
                      >
                        {user.isActive ? 'Deactivate' : 'Activate'}
                      </button>
                      <button
                        className="btn-delete"
                        onClick={() => handleDelete(user.userId)}
                      >
                        Delete
                      </button>
                    </div>
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>

        {filteredUsers.length === 0 && (
          <div className="empty-state">
            <p>No users found.</p>
          </div>
        )}
      </div>
    </div>
  );
};

export default Users;
