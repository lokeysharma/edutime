import React, { useEffect, useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { usersApi, coursesApi, enrollmentsApi, assignmentsApi } from '../../services/api';
import './Dashboard.css';

const Dashboard: React.FC = () => {
  const { user, isAdmin, isTeacher, isStudent } = useAuth();
  const [stats, setStats] = useState<any>({});
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadDashboardData();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const loadDashboardData = async () => {
    try {
      const profile = await usersApi.getMe();
      
      if (isAdmin) {
        const [users, courses] = await Promise.all([
          usersApi.getAll(),
          coursesApi.getAll(),
        ]);
        setStats({
          profile: profile.data,
          totalUsers: users.data.length,
          totalCourses: courses.data.length,
          teachers: users.data.filter((u: any) => u.role === 'Teacher').length,
          students: users.data.filter((u: any) => u.role === 'Student').length,
        });
      } else if (isTeacher) {
        const courses = await coursesApi.getMyCourses();
        const assignments = await assignmentsApi.getMyAssignments();
        setStats({
          profile: profile.data,
          myCourses: courses.data.length,
          totalStudents: courses.data.reduce((sum: number, c: any) => sum + (c.enrollmentsCount || 0), 0),
          myAssignments: assignments.data.length,
        });
      } else if (isStudent) {
        const enrollments = await enrollmentsApi.getMyEnrollments();
        setStats({
          profile: profile.data,
          enrolledCourses: enrollments.data.length,
          activeCourses: enrollments.data.filter((e: any) => e.status === 'Active').length,
        });
      }
    } catch (error) {
      console.error('Failed to load dashboard data:', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return <div className="loading">Loading dashboard...</div>;
  }

  return (
    <div className="dashboard">
      <div className="dashboard-header">
        <h1>Welcome, {stats.profile?.firstName || user?.email}!</h1>
        <p className="role-badge">{user?.role}</p>
      </div>

      <div className="stats-grid">
        {isAdmin && (
          <>
            <div className="stat-card">
              <div className="stat-icon">👥</div>
              <div className="stat-info">
                <h3>{stats.totalUsers}</h3>
                <p>Total Users</p>
              </div>
            </div>
            <div className="stat-card">
              <div className="stat-icon">📚</div>
              <div className="stat-info">
                <h3>{stats.totalCourses}</h3>
                <p>Total Courses</p>
              </div>
            </div>
            <div className="stat-card">
              <div className="stat-icon">👨‍🏫</div>
              <div className="stat-info">
                <h3>{stats.teachers}</h3>
                <p>Teachers</p>
              </div>
            </div>
            <div className="stat-card">
              <div className="stat-icon">👨‍🎓</div>
              <div className="stat-info">
                <h3>{stats.students}</h3>
                <p>Students</p>
              </div>
            </div>
          </>
        )}

        {isTeacher && (
          <>
            <div className="stat-card">
              <div className="stat-icon">📚</div>
              <div className="stat-info">
                <h3>{stats.myCourses}</h3>
                <p>My Courses</p>
              </div>
            </div>
            <div className="stat-card">
              <div className="stat-icon">👨‍🎓</div>
              <div className="stat-info">
                <h3>{stats.totalStudents}</h3>
                <p>Total Students</p>
              </div>
            </div>
            <div className="stat-card">
              <div className="stat-icon">📝</div>
              <div className="stat-info">
                <h3>{stats.myAssignments}</h3>
                <p>Assignments</p>
              </div>
            </div>
          </>
        )}

        {isStudent && (
          <>
            <div className="stat-card">
              <div className="stat-icon">📚</div>
              <div className="stat-info">
                <h3>{stats.enrolledCourses}</h3>
                <p>Enrolled Courses</p>
              </div>
            </div>
            <div className="stat-card">
              <div className="stat-icon">✅</div>
              <div className="stat-info">
                <h3>{stats.activeCourses}</h3>
                <p>Active Courses</p>
              </div>
            </div>
          </>
        )}
      </div>

      <div className="profile-section">
        <h2>Profile Information</h2>
        <div className="profile-card">
          <div className="profile-item">
            <span className="label">Email:</span>
            <span>{stats.profile?.email}</span>
          </div>
          <div className="profile-item">
            <span className="label">Name:</span>
            <span>{stats.profile?.firstName} {stats.profile?.lastName}</span>
          </div>
          <div className="profile-item">
            <span className="label">Role:</span>
            <span>{stats.profile?.role}</span>
          </div>
          <div className="profile-item">
            <span className="label">Phone:</span>
            <span>{stats.profile?.phone || 'Not provided'}</span>
          </div>
          <div className="profile-item">
            <span className="label">Member Since:</span>
            <span>{new Date(stats.profile?.createdAt).toLocaleDateString()}</span>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
