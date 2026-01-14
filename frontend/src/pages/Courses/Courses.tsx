import React, { useEffect, useState } from 'react';
import { coursesApi, usersApi } from '../../services/api';
import { useAuth } from '../../context/AuthContext';
import { Course } from '../../types';
import './Courses.css';

const Courses: React.FC = () => {
  const [courses, setCourses] = useState<Course[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [teachers, setTeachers] = useState<any[]>([]);
  const [editingCourse, setEditingCourse] = useState<Course | null>(null);
  const [formData, setFormData] = useState({
    courseCode: '',
    courseName: '',
    description: '',
    credits: 3,
    teacherId: 0,
  });
  const { isAdmin, isTeacher } = useAuth();

  useEffect(() => {
    const loadInitialData = async () => {
      try {
        const response = isTeacher 
          ? await coursesApi.getMyCourses()
          : await coursesApi.getAll();
        setCourses(response.data);
      } catch (error) {
        console.error('Failed to load courses:', error);
      } finally {
        setLoading(false);
      }

      if (isAdmin || isTeacher) {
        try {
          const teachersResponse = await usersApi.getTeachers();
          setTeachers(teachersResponse.data);
        } catch (error) {
          console.error('Failed to load teachers:', error);
        }
      }
    };
    loadInitialData();
  }, [isAdmin, isTeacher]);

  const loadCourses = async () => {
    try {
      const response = isTeacher 
        ? await coursesApi.getMyCourses()
        : await coursesApi.getAll();
      setCourses(response.data);
    } catch (error) {
      console.error('Failed to load courses:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Validate teacherId
    if (!formData.teacherId || formData.teacherId === 0) {
      alert('Please select a teacher');
      return;
    }
    
    try {
      if (editingCourse) {
        await coursesApi.update(editingCourse.courseId, formData);
      } else {
        await coursesApi.create(formData);
      }
      setShowModal(false);
      setEditingCourse(null);
      resetForm();
      loadCourses();
    } catch (error: any) {
      alert(error.response?.data?.message || 'Failed to save course');
    }
  };

  const handleEdit = (course: Course) => {
    setEditingCourse(course);
    setFormData({
      courseCode: course.courseCode,
      courseName: course.courseName,
      description: course.description,
      credits: course.credits,
      teacherId: course.teacherId,
    });
    setShowModal(true);
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this course?')) {
      try {
        await coursesApi.delete(id);
        loadCourses();
      } catch (error: any) {
        alert(error.response?.data?.message || 'Failed to delete course');
      }
    }
  };

  const resetForm = () => {
    setFormData({
      courseCode: '',
      courseName: '',
      description: '',
      credits: 3,
      teacherId: teachers[0]?.userId || 0,
    });
  };

  const openAddModal = () => {
    setEditingCourse(null);
    resetForm();
    setShowModal(true);
  };

  if (loading) {
    return <div className="loading">Loading courses...</div>;
  }

  return (
    <div className="courses-page">
      <div className="page-header">
        <h1>📚 Courses</h1>
        {(isAdmin || isTeacher) && (
          <button className="btn-add" onClick={openAddModal}>
            + Add Course
          </button>
        )}
      </div>

      <div className="courses-grid">
        {courses.map((course) => (
          <div key={course.courseId} className="course-card">
            <div className="course-header">
              <span className="course-code">{course.courseCode}</span>
              <span className={`status ${course.isActive ? 'active' : 'inactive'}`}>
                {course.isActive ? 'Active' : 'Inactive'}
              </span>
            </div>
            <h3>{course.courseName}</h3>
            <p className="description">{course.description}</p>
            <div className="course-meta">
              <span>📊 {course.credits} Credits</span>
              {course.teacher && (
                <span>👨‍🏫 {course.teacher.firstName} {course.teacher.lastName}</span>
              )}
            </div>
            <div className="course-stats">
              <span>📅 {course.sessionsCount || 0} Sessions</span>
              <span>👥 {course.enrollmentsCount || 0} Students</span>
            </div>
            {(isAdmin || isTeacher) && (
              <div className="course-actions">
                <button className="btn-edit" onClick={() => handleEdit(course)}>Edit</button>
                {isAdmin && (
                  <button className="btn-delete" onClick={() => handleDelete(course.courseId)}>Delete</button>
                )}
              </div>
            )}
          </div>
        ))}
      </div>

      {courses.length === 0 && (
        <div className="empty-state">
          <p>No courses found.</p>
        </div>
      )}

      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h2>{editingCourse ? 'Edit Course' : 'Add New Course'}</h2>
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Course Code</label>
                <input
                  type="text"
                  value={formData.courseCode}
                  onChange={(e) => setFormData({ ...formData, courseCode: e.target.value })}
                  placeholder="e.g., CS101"
                  required
                />
              </div>
              <div className="form-group">
                <label>Course Name</label>
                <input
                  type="text"
                  value={formData.courseName}
                  onChange={(e) => setFormData({ ...formData, courseName: e.target.value })}
                  placeholder="e.g., Introduction to Programming"
                  required
                />
              </div>
              <div className="form-group">
                <label>Description</label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  placeholder="Course description..."
                  required
                />
              </div>
              <div className="form-row">
                <div className="form-group">
                  <label>Credits</label>
                  <input
                    type="number"
                    value={formData.credits}
                    onChange={(e) => setFormData({ ...formData, credits: parseInt(e.target.value) })}
                    min="1"
                    max="10"
                    required
                  />
                </div>
                <div className="form-group">
                  <label>Teacher</label>
                  <select
                    value={formData.teacherId || ''}
                    onChange={(e) => setFormData({ ...formData, teacherId: parseInt(e.target.value) || 0 })}
                    required
                  >
                    <option value="">Select Teacher</option>
                    {teachers.map((teacher) => (
                      <option key={teacher.userId} value={teacher.userId}>
                        {teacher.firstName} {teacher.lastName}
                      </option>
                    ))}
                  </select>
                </div>
              </div>
              <div className="modal-actions">
                <button type="button" className="btn-cancel" onClick={() => setShowModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn-save">
                  {editingCourse ? 'Update' : 'Create'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Courses;
