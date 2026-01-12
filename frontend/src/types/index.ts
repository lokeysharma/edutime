export interface User {
  userId: number;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  phone?: string;
  createdAt: string;
  lastLoginAt?: string;
  isActive: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  email: string;
  role: string;
  expiresAt: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phone?: string;
  role: string;
}

export interface Course {
  courseId: number;
  courseCode: string;
  courseName: string;
  description: string;
  credits: number;
  teacherId: number;
  createdAt: string;
  isActive: boolean;
  teacher?: {
    userId: number;
    firstName: string;
    lastName: string;
    email: string;
  };
  sessionsCount?: number;
  enrollmentsCount?: number;
}

export interface ClassSession {
  id: number;
  sessionName: string;
  courseId?: number;
  sessionDate?: string;
  description?: string;
  isActive: boolean;
  course?: {
    courseId: number;
    courseCode: string;
    courseName: string;
  };
}

export interface Enrollment {
  enrollmentId: number;
  studentId: number;
  courseId: number;
  enrolledAt: string;
  status: string;
  student?: User;
  course?: Course;
}

export interface Assignment {
  assignmentId: number;
  courseId: number;
  title: string;
  description: string;
  dueDate: string;
  maxScore: number;
  createdAt: string;
  isPublished: boolean;
  course?: {
    courseId: number;
    courseCode: string;
    courseName: string;
  };
  submissionsCount?: number;
}

export interface Submission {
  submissionId: number;
  assignmentId: number;
  studentId: number;
  content: string;
  fileUrl?: string;
  submittedAt: string;
  score?: number;
  feedback?: string;
  gradedAt?: string;
  student?: User;
  assignment?: Assignment;
}

export interface Attendance {
  attendanceId: number;
  studentId: number;
  classSessionId: number;
  date: string;
  status: string;
  notes?: string;
  recordedAt: string;
  student?: User;
  session?: ClassSession;
}
