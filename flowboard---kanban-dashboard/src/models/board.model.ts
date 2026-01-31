
export interface User {
  name: string;
  avatarUrl: string;
}

export interface Task {
  id: string;
  title: string;
  tags: { name: string; color: string; textColor: string }[];
  assignees: User[];
  commentsCount: number;
  attachmentsCount: number;
  dueDate?: string;
  progress?: number;
  completed: boolean;
  description: string;
  checklist: { item: string; completed: boolean }[];
  status: string;
}

export interface Column {
  id: string;
  title: string;
  tasks: string[]; // array of task ids
  color: string;
}

export interface Board {
  id: string;
  title: string;
  lastModified: string;
  members: User[];
  moreMembersCount: number;
  progress: number;
  imageUrl: string;
  columns: Column[];
  tasks: Task[];
}
