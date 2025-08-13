namespace Ai.Tutor.Domain.Enums;

public enum ThreadStatus { Active, Archived, Deleted }
public enum MessageStatus { Sending, Sent, Error }
public enum SenderType { User, Ai }
public enum FolderType { Project, Folder }
public enum FolderStatus { Active, Archived, Deleted }
public enum AttachmentType { Document, Image, Other }
public enum ReferenceType { File, Page, Video, Link, Formula }
public enum EventType { MessageCreated, MessageUpdated, MessageDeleted }
public enum OrgType { Personal, Education, Household, Business }
public enum OrgRole { Owner, Admin, Member }
