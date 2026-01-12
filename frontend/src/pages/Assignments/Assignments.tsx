import React, { useEffect, useState } from 'react';
import { assignmentsApi, coursesApi, submissionsApi } from '../../services/api';
import { useAuth } from '../../context/AuthContext';
import { Assignment, Course } from '../../types';
import './Assignments.css';

const Assignments: React.FC = () => {
  const [assignments, setAssignments] = useState<Assignment[]>([]);
  const [courses, setCourses] = useState<Course[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [showSubmitModal, setShowSubmitModal] = useState(false);
  const [selectedAssignment, setSelectedAssignment] = useState<Assignment | null>(null);
  const [formData, setFormData] = useState({
    courseId: 0,
    title: '',
    description: '',
    dueDate: '',
    maxScore: 100,
  });
  const [submitData, setSubmitData] = useState({
    content: '',
    fileUrl: '',
  });
  const { isAdmin, isTeacher, isStudent } = useAuth();

  useEffect(() => {
    loadAssignments();
    if (isAdmin || isTeacher) {
      loadCourses();
    }
  }, []);

  const loadAssignments = async () => {
    try {
      const response = isTeacher
        ? await assignmentsApi.getMyAssignments()
        : await assignmentsApi.getAll();
      setAssignments(response.data);
    } catch (error) {
      console.error('Failed to load assignments:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadCourses = async () => {
    try {
      const response = isTeacher
        ? await coursesApi.getMyCourses()
        : await coursesApi.getAll();
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
        dueDate: new Date(formData.dueDate).toISOString(),
      };
      await assignmentsApi.create(data);
      setShowModal(false);
      resetForm();
      loadAssignments();
    } catch (error: any) {
      alert(error.response?.data?.message || 'Failed to create assignment');
    }
  };

  const handleSubmitAssignment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedAssignment) return;
    
    try {
      await submissionsApi.submit({
        assignmentId: selectedAssignment.assignmentId,
        content: submitData.content,
        fileUrl: submitData.fileUrl || null,
      });
      setShowSubmitModal(false);
      setSubmitData({ content: '', fileUrl: '' });
      alert('Assignment submitted successfully!');
    } catch (error: any) {
      alert(error.response?.data?.message || 'Failed to submit assignment');
    }
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this assignment?')) {
      try {
        await assignmentsApi.delete(id);
        loadAssignments();
      } catch (error: any) {
        alert(error.response?.data?.message || 'Failed to delete assignment');
      }
    }
  };

  const resetForm = () => {
    setFormData({
      courseId: courses[0]?.courseId || 0,
      title: '',
      description: '',
      dueDate: '',
      maxScore: 100,
    });
  };

  const openAddModal = () => {
    resetForm();
    setShowModal(true);
  };

  const openSubmitModal = (assignment: Assignment) => {
    setSelectedAssignment(assignment);
    setShowSubmitModal(true);
  };

  const isOverdue = (dueDate: string) => new Date(dueDate) < new Date();

  if (loading) {
    return <div className="loading">Loading assignments...</div>;
  }

  return (
    <div className="assignments-page">
      <div className="page-header">
        <h1>📝 Assignments</h1>
        {(isAdmin || isTeacher) && (
          <button className="btn-add" onClick={openAddModal}>
            + Add Assignment
          </button>
        )}
      </div>

      <div className="assignments-grid">
        {assignments.map((assignment) => (
          <div key={assignment.assignmentId} className="assignment-card">
            <div className="assignment-header">
              <span className={`due-badge ${isOverdue(assignment.dueDate) ? 'overdue' : ''}`}>
                {isOverdue(assignment.dueDate) ? 'Overdue' : 'Due'}: {new Date(assignment.dueDate).toLocaleDateString()}
              </span>
              <span className="score-badge">{assignment.maxScore} pts</span>
            </div>
            <h3>{assignment.title}</h3>
            <p className="description">{assignment.description}</p>
            {assignment.course && (
              <div className="course-tag">
                📚 {assignment.course.courseCode} - {assignment.course.courseName}
              </div>
            )}
            <div className="assignment-footer">
              {(isAdmin || isTeacher) && (
                <>
                  <span className="submissions-count">
                    {assignment.submissionsCount || 0} submissions
                  </span>
                  <div className="action-buttons">
                    <button className="btn-delete" onClick={() => handleDelete(assignment.assignmentId)}>
                      Delete
                    </button>
                  </div>
                </>
              )}
              {isStudent && !isOverdue(assignment.dueDate) && (
                <button className="btn-submit" onClick={() => openSubmitModal(assignment)}>
                  Submit
                </button>
              )}
            </div>
          </div>
        ))}
      </div>

      {assignments.length === 0 && (
        <div className="empty-state">
          <p>No assignments found.</p>
        </div>
      )}

      {/* Create Assignment Modal */}
      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h2>Create New Assignment</h2>
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Course</label>
                <select
                  value={formData.courseId}
                  onChange={(e) => setFormData({ ...formData, courseId: parseInt(e.target.value) })}
                  required
                >
                  <option value="">Select Course</option>
                  {courses.map((course) => (
                    <option key={course.courseId} value={course.courseId}>
                      {course.courseCode} - {course.courseName}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label>Title</label>
                <input
                  type="text"
                  value={formData.title}
                  onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                  placeholder="Assignment title"
                  required
                />
              </div>
              <div className="form-group">
                <label>Description</label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  placeholder="Assignment description..."
                  required
                />
              </div>
              <div className="form-row">
                <div className="form-group">
                  <label>Due Date</label>
                  <input
                    type="date"
                    value={formData.dueDate}
                    onChange={(e) => setFormData({ ...formData, dueDate: e.target.value })}
                    required
                  />
                </div>
                <div className="form-group">
                  <label>Max Score</label>
                  <input
                    type="number"
                    value={formData.maxScore}
                    onChange={(e) => setFormData({ ...formData, maxScore: parseInt(e.target.value) })}
                    min="1"
                    required
                  />
                </div>
              </div>
              <div className="modal-actions">
                <button type="button" className="btn-cancel" onClick={() => setShowModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn-save">Create</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Submit Assignment Modal */}
      {showSubmitModal && selectedAssignment && (
        <div className="modal-overlay" onClick={() => setShowSubmitModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h2>Submit: {selectedAssignment.title}</h2>
            <form onSubmit={handleSubmitAssignment}>
              <div className="form-group">
                <label>Your Answer</label>
                <textarea
                  value={submitData.content}
                  onChange={(e) => setSubmitData({ ...submitData, content: e.target.value })}
                  placeholder="Write your answer here..."
                  required
                  rows={6}
                />
              </div>
              <div className="form-group">
                <label>File URL (Optional)</label>
                <input
                  type="url"
                  value={submitData.fileUrl}
                  onChange={(e) => setSubmitData({ ...submitData, fileUrl: e.target.value })}
                  placeholder="https://..."
                />
              </div>
              <div className="modal-actions">
                <button type="button" className="btn-cancel" onClick={() => setShowSubmitModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn-save">Submit</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Assignments;
