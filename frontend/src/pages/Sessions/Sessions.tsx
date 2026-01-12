import React, { useEffect, useState } from 'react';
import { sessionsApi, coursesApi } from '../../services/api';
import { useAuth } from '../../context/AuthContext';
import { ClassSession, Course } from '../../types';
import './Sessions.css';

const Sessions: React.FC = () => {
  const [sessions, setSessions] = useState<ClassSession[]>([]);
  const [courses, setCourses] = useState<Course[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editingSession, setEditingSession] = useState<ClassSession | null>(null);
  const [formData, setFormData] = useState({
    sessionName: '',
    courseId: null as number | null,
    sessionDate: '',
    description: '',
  });
  const { isAdmin, isTeacher } = useAuth();

  useEffect(() => {
    loadSessions();
    loadCourses();
  }, []);

  const loadSessions = async () => {
    try {
      const response = await sessionsApi.getAll();
      setSessions(response.data);
    } catch (error) {
      console.error('Failed to load sessions:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadCourses = async () => {
    try {
      const response = await coursesApi.getAll();
      setCourses(response.data);
    } catch (error) {
      console.error('Failed to load courses:', error);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const data = {
        ...formData,
        courseId: formData.courseId || null,
        sessionDate: formData.sessionDate ? new Date(formData.sessionDate).toISOString() : null,
      };
      
      if (editingSession) {
        await sessionsApi.update(editingSession.id, data);
      } else {
        await sessionsApi.create(data);
      }
      setShowModal(false);
      setEditingSession(null);
      resetForm();
      loadSessions();
    } catch (error: any) {
      alert(error.response?.data?.message || 'Failed to save session');
    }
  };

  const handleEdit = (session: ClassSession) => {
    setEditingSession(session);
    setFormData({
      sessionName: session.sessionName,
      courseId: session.courseId || null,
      sessionDate: session.sessionDate ? session.sessionDate.split('T')[0] : '',
      description: session.description || '',
    });
    setShowModal(true);
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this session?')) {
      try {
        await sessionsApi.delete(id);
        loadSessions();
      } catch (error: any) {
        alert(error.response?.data?.message || 'Failed to delete session');
      }
    }
  };

  const resetForm = () => {
    setFormData({
      sessionName: '',
      courseId: null,
      sessionDate: '',
      description: '',
    });
  };

  const openAddModal = () => {
    setEditingSession(null);
    resetForm();
    setShowModal(true);
  };

  if (loading) {
    return <div className="loading">Loading sessions...</div>;
  }

  return (
    <div className="sessions-page">
      <div className="page-header">
        <h1>📅 Class Sessions</h1>
        {(isAdmin || isTeacher) && (
          <button className="btn-add" onClick={openAddModal}>
            + Add Session
          </button>
        )}
      </div>

      <div className="sessions-list">
        <table>
          <thead>
            <tr>
              <th>Session Name</th>
              <th>Course</th>
              <th>Date</th>
              <th>Status</th>
              {(isAdmin || isTeacher) && <th>Actions</th>}
            </tr>
          </thead>
          <tbody>
            {sessions.map((session) => (
              <tr key={session.id}>
                <td>
                  <strong>{session.sessionName}</strong>
                  {session.description && (
                    <p className="session-desc">{session.description}</p>
                  )}
                </td>
                <td>
                  {session.course ? (
                    <span className="course-badge">
                      {session.course.courseCode} - {session.course.courseName}
                    </span>
                  ) : (
                    <span className="no-course">No course</span>
                  )}
                </td>
                <td>
                  {session.sessionDate
                    ? new Date(session.sessionDate).toLocaleDateString()
                    : 'Not scheduled'}
                </td>
                <td>
                  <span className={`status ${session.isActive ? 'active' : 'inactive'}`}>
                    {session.isActive ? 'Active' : 'Inactive'}
                  </span>
                </td>
                {(isAdmin || isTeacher) && (
                  <td>
                    <div className="action-buttons">
                      <button className="btn-edit" onClick={() => handleEdit(session)}>
                        Edit
                      </button>
                      {isAdmin && (
                        <button className="btn-delete" onClick={() => handleDelete(session.id)}>
                          Delete
                        </button>
                      )}
                    </div>
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>

        {sessions.length === 0 && (
          <div className="empty-state">
            <p>No sessions found.</p>
          </div>
        )}
      </div>

      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h2>{editingSession ? 'Edit Session' : 'Add New Session'}</h2>
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Session Name</label>
                <input
                  type="text"
                  value={formData.sessionName}
                  onChange={(e) => setFormData({ ...formData, sessionName: e.target.value })}
                  placeholder="e.g., Week 1 - Introduction"
                  required
                />
              </div>
              <div className="form-group">
                <label>Course (Optional)</label>
                <select
                  value={formData.courseId || ''}
                  onChange={(e) => setFormData({ ...formData, courseId: e.target.value ? parseInt(e.target.value) : null })}
                >
                  <option value="">No Course</option>
                  {courses.map((course) => (
                    <option key={course.courseId} value={course.courseId}>
                      {course.courseCode} - {course.courseName}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label>Session Date</label>
                <input
                  type="date"
                  value={formData.sessionDate}
                  onChange={(e) => setFormData({ ...formData, sessionDate: e.target.value })}
                />
              </div>
              <div className="form-group">
                <label>Description</label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  placeholder="Session description..."
                />
              </div>
              <div className="modal-actions">
                <button type="button" className="btn-cancel" onClick={() => setShowModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn-save">
                  {editingSession ? 'Update' : 'Create'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Sessions;
