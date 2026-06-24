#!/usr/bin/env python3
"""Generates en.json and ar.json seed files with consistent relationships."""
import json
from pathlib import Path

OUT = Path(__file__).parent

def en_data():
    return {
        "president": "Abdullah Mohammed Al Naqool",
        "landingSlides": [
            {"titleLine1": "United by Heritage,", "titleLine2": "Connected by Family", "subtitle": "The official app of Al Naqool family — news, events, and council updates in one place."},
            {"titleLine1": "From Riyadh to Jeddah,", "titleLine2": "One Extended Family", "subtitle": "Explore branches, meet members, and follow your lineage across generations."},
            {"titleLine1": "Council Decisions,", "titleLine2": "Transparent to All", "subtitle": "Meetings, committees, voting, and documents — accessible to every member."},
        ],
        "homeStats": {"totalMembers": 2486, "familyBranches": 6, "newsUpdates": 8, "upcomingEvents": 6},
        "newsCategories": ["All", "General", "Births", "Weddings", "Condolences", "Council"],
        "messageFilters": ["All", "Announcements", "Groups", "Direct"],
        "galleryTypes": ["Photos", "Videos", "Albums", "Heritage"],
        "documentCategories": ["All", "Council", "Family Records", "Legal", "Financial"],
        "directoryBranches": ["All", "Al Mofreh", "Al Dhafer", "Al Rashid", "Al Saeed", "Al Hamad", "Al Naqool"],
        "termsEn": "By registering you agree to the Al Naqool family app terms, privacy policy, and council bylaws. Member data is used only for family communication.",
        "termsAr": "بالتسجيل فإنك توافق على شروط تطبيق عائلة النقول وسياسة الخصوصية ولوائح المجلس. تُستخدم بيانات الأعضاء للتواصل العائلي فقط.",
        "branches": [
            {"key": "mofreh", "name": "Al Mofreh", "memberCount": 412, "description": "The largest branch, based in Riyadh. Known for organizing annual gatherings and youth programs.", "imageUrl": "https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?w=400"},
            {"key": "dhafer", "name": "Al Dhafer", "memberCount": 318, "description": "Eastern Province branch with a strong role in social support and condolence coordination.", "imageUrl": "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400"},
            {"key": "rashid", "name": "Al Rashid", "memberCount": 285, "description": "Makkah branch active in education scholarships and Hajj season coordination.", "imageUrl": "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=400"},
            {"key": "saeed", "name": "Al Saeed", "memberCount": 264, "description": "Medina branch leading genealogy documentation and the family digital archive.", "imageUrl": "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=400"},
            {"key": "hamad", "name": "Al Hamad", "memberCount": 231, "description": "Southern branch focused on welfare funds and supporting elderly members.", "imageUrl": "https://images.unsplash.com/photo-1612349317150-e413f6a5b16d?w=400"},
            {"key": "naqool", "name": "Al Naqool", "memberCount": 198, "description": "The founding line — council presidency and central administration.", "imageUrl": "https://images.unsplash.com/photo-1560250097-0b93528c311a?w=400"},
        ],
        "users": [
            {"key": "admin", "userName": "admin", "email": "admin@alnaqool.com", "password": "Admin123!", "fullName": "System Administrator", "displayRole": "Administrator", "memberId": "ADMIN-001", "branchKey": "naqool", "phone": "+966500000001", "role": "Admin", "accountStatus": "Active", "dateOfBirth": "January 1, 1980", "maritalStatus": "Married", "childrenCount": 0},
            {"key": "ahmed", "userName": "ahmed", "email": "ahmed.alnaqool@email.com", "password": "password123", "fullName": "Ahmed Abdullah Al Naqool", "displayRole": "Family Member", "memberId": "AN-2026-0847", "branchKey": "mofreh", "phone": "+966501234567", "role": "Member", "accountStatus": "Active", "dateOfBirth": "March 15, 1985", "maritalStatus": "Married", "childrenCount": 3},
            {"key": "mohammed", "userName": "mohammed.mofreh", "email": "mohammed.mofreh@alnaqool.com", "password": "password123", "fullName": "Mohammed Salman Al Mofreh", "displayRole": "Branch Head", "memberId": "AN-2026-0101", "branchKey": "mofreh", "phone": "+966501112233", "role": "BranchHead", "accountStatus": "Active", "dateOfBirth": "August 12, 1972", "maritalStatus": "Married", "childrenCount": 5},
            {"key": "khalid", "userName": "khalid.dhafer", "email": "khalid.dhafer@alnaqool.com", "password": "password123", "fullName": "Khalid Ibrahim Al Dhafer", "displayRole": "Branch Head", "memberId": "AN-2026-0201", "branchKey": "dhafer", "phone": "+966504445566", "role": "BranchHead", "accountStatus": "Active", "dateOfBirth": "May 3, 1975", "maritalStatus": "Married", "childrenCount": 4},
            {"key": "noura", "userName": "noura.saeed", "email": "noura.saeed@alnaqool.com", "password": "password123", "fullName": "Noura Fahad Al Saeed", "displayRole": "Secretary", "memberId": "AN-2026-0401", "branchKey": "saeed", "phone": "+966507778899", "role": "Secretary", "accountStatus": "Active", "dateOfBirth": "November 22, 1988", "maritalStatus": "Married", "childrenCount": 2},
            {"key": "omar", "userName": "omar.mofreh", "email": "omar.mofreh@alnaqool.com", "password": "password123", "fullName": "Omar Mohammed Al Mofreh", "displayRole": "Committee Organizer", "memberId": "AN-2026-0103", "branchKey": "mofreh", "phone": "+966503334455", "role": "CommitteeOrganizer", "accountStatus": "Active", "dateOfBirth": "June 9, 1995", "maritalStatus": "Single", "childrenCount": 0},
            {"key": "president", "userName": "abdullah.president", "email": "abdullah.president@alnaqool.com", "password": "password123", "fullName": "Abdullah Mohammed Al Naqool", "displayRole": "Council President", "memberId": "AN-2026-0001", "branchKey": "naqool", "phone": "+966509998877", "role": "CouncilPresident", "accountStatus": "Active", "dateOfBirth": "February 14, 1968", "maritalStatus": "Married", "childrenCount": 6},
            {"key": "pending_faisal", "userName": "faisal.new", "email": "faisal.rashid@gmail.com", "password": "password123", "fullName": "Faisal Turki Al Rashid", "displayRole": "Family Member", "memberId": "PENDING-001", "branchKey": "rashid", "phone": "+966506667788", "role": "Member", "accountStatus": "Pending", "registrationRelation": "Son of Abdullah Al Rashid", "dateOfBirth": "April 18, 1998", "maritalStatus": "Single", "childrenCount": 0},
        ],
        "newsItems": [
            {"category": "Council", "title": "Q2 Family Council Meeting — June 28", "description": "Agenda published for the quarterly council session in Riyadh.", "body": "The Family Council invites all branch representatives to the Q2 meeting at Al Naqool Heritage Center. Topics: annual budget, youth fund, and Eid gathering planning.", "publishedDate": "June 20, 2026", "categoryColorHex": "0xFF00332B", "imageUrl": "https://images.unsplash.com/photo-1475721027785-f74eccf877e2?w=800", "isFeatured": True, "publishStatus": "Published"},
            {"category": "Weddings", "title": "Wedding of Turki Al Hamad & Layla Al Saeed", "description": "Celebration in Abha on July 12, 2026.", "body": "The family congratulates Turki and Layla. Men's reception at 9 PM; women's gathering at Al Faisaliah Hall. RSVP through the Events section.", "publishedDate": "June 18, 2026", "categoryColorHex": "0xFF8B5A2B", "imageUrl": "https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=800", "isFeatured": True, "publishStatus": "Published"},
            {"category": "Births", "title": "Welcome Rayan — Al Mofreh Branch", "description": "Birth announcement: baby boy to Ahmed and Hessa Al Mofreh.", "body": "We thank Allah for the safe arrival of Rayan. Mother and child are well. Aqiqah details will be shared by the branch head.", "publishedDate": "June 15, 2026", "categoryColorHex": "0xFF2E7D32", "imageUrl": "https://images.unsplash.com/photo-1578662996442-48f60103fc96?w=800", "isFeatured": False, "publishStatus": "Published"},
            {"category": "General", "title": "Annual Family Gathering — Save the Date", "description": "Mark December 18, 2026 on your calendar.", "body": "This year's gathering returns to Riyadh with heritage exhibitions, children's corner, and recognition of community contributors.", "publishedDate": "June 10, 2026", "categoryColorHex": "0xFF546E7A", "imageUrl": "https://images.unsplash.com/photo-1758985776354-4df674930917?w=800", "isFeatured": True, "publishStatus": "Published"},
            {"category": "Condolences", "title": "Condolences to Al Dhafer Branch", "description": "Passing of Sheikh Ibrahim Al Dhafer — may he rest in peace.", "body": "The council extends condolences to the Al Dhafer branch. Prayer times and burial location shared with branch members. Charity fund link in Documents.", "publishedDate": "June 5, 2026", "categoryColorHex": "0xFF37474F", "imageUrl": "https://images.unsplash.com/photo-1770786106021-52580470e31e?w=800", "isFeatured": False, "publishStatus": "Published"},
            {"category": "Council", "title": "Youth Leadership Program Launched", "description": "Applications open for ages 18–30.", "body": "The Youth Committee announces a six-month leadership program covering heritage, governance, and community service. Apply before July 1.", "publishedDate": "May 28, 2026", "categoryColorHex": "0xFF1565C0", "imageUrl": "https://images.unsplash.com/photo-1529156069898-49953e39b3ac?w=800", "isFeatured": False, "publishStatus": "Published"},
            {"category": "General", "title": "Updated Family Directory Published", "description": "2026 directory now available in the app.", "body": "Branch heads verified contact details. Members can update their profile or request corrections through the secretary.", "publishedDate": "May 20, 2026", "categoryColorHex": "0xFF546E7A", "imageUrl": "https://images.unsplash.com/photo-1517245386807-bb43f82c33c4?w=800", "isFeatured": False, "publishStatus": "Published"},
            {"category": "Births", "title": "Congratulations — Al Rashid Branch", "description": "Twin girls born to Faisal and Reem Al Rashid.", "body": "Named Joud and Layan. The family requests prayers and shares joy with all branches.", "publishedDate": "May 12, 2026", "categoryColorHex": "0xFF2E7D32", "imageUrl": "https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?w=800", "isFeatured": False, "publishStatus": "Published"},
        ],
        "events": [
            {"day": "28", "month": "Jun", "title": "Q2 Council Meeting", "location": "Al Naqool Heritage Center, Riyadh", "time": "4:00 PM", "fullDate": "June 28, 2026", "description": "Quarterly council session — budget review, committee reports, and Eid planning.", "organizer": "Family Council", "organizerUserKey": "president", "committeeKey": "council", "isPublic": False, "isMine": True},
            {"day": "05", "month": "Jul", "title": "Youth Workshop — Leadership & Heritage", "location": "King Abdulaziz Center, Riyadh", "time": "10:00 AM", "fullDate": "July 5, 2026", "description": "Interactive workshop for members aged 18–30. Registration required.", "organizer": "Youth Committee", "organizerUserKey": "omar", "committeeKey": "youth", "isPublic": True, "isMine": True},
            {"day": "12", "month": "Jul", "title": "Wedding Reception — Turki & Layla", "location": "Abha — Al Faisaliah Hall", "time": "9:00 PM", "fullDate": "July 12, 2026", "description": "Family celebration for Turki Al Hamad and Layla Al Saeed.", "organizer": "Social Committee", "organizerUserKey": "mohammed", "committeeKey": "events", "isPublic": True, "isMine": False},
            {"day": "20", "month": "Aug", "title": "Heritage Archive Open Day", "location": "Medina — Al Saeed Family House", "time": "5:00 PM", "fullDate": "August 20, 2026", "description": "Tour the digitized genealogy archive with the Heritage Committee.", "organizer": "Heritage Committee", "organizerUserKey": "noura", "committeeKey": "heritage", "isPublic": True, "isMine": False},
            {"day": "15", "month": "Sep", "title": "Eastern Province Branch Reunion", "location": "Dammam Corniche Pavilion", "time": "7:00 PM", "fullDate": "September 15, 2026", "description": "Al Dhafer branch annual reunion — dinner and children's activities.", "organizer": "Al Dhafer Branch", "organizerUserKey": "khalid", "committeeKey": "dhafer", "isPublic": True, "isMine": False},
            {"day": "18", "month": "Dec", "title": "Annual Family Gathering 2026", "location": "Riyadh — Diplomatic Quarter", "time": "6:00 PM", "fullDate": "December 18, 2026", "description": "The flagship annual gathering for all branches.", "organizer": "Events Committee", "organizerUserKey": "mohammed", "committeeKey": "events", "isPublic": True, "isMine": True},
        ],
        "branchMembers": [
            {"name": "Mohammed Salman Al Mofreh", "role": "Branch Head", "branchKey": "mofreh", "imageUrl": "https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?w=200"},
            {"name": "Sara Hamed Al Mofreh", "role": "Secretary", "branchKey": "mofreh", "imageUrl": "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=200"},
            {"name": "Omar Mohammed Al Mofreh", "role": "Youth Representative", "branchKey": "mofreh", "imageUrl": "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=200"},
            {"name": "Khalid Ibrahim Al Dhafer", "role": "Branch Head", "branchKey": "dhafer", "imageUrl": "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=200"},
            {"name": "Fatima Nasser Al Dhafer", "role": "Social Affairs", "branchKey": "dhafer", "imageUrl": "https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=200"},
            {"name": "Abdullah Turki Al Rashid", "role": "Branch Head", "branchKey": "rashid", "imageUrl": "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=200"},
            {"name": "Noura Fahad Al Saeed", "role": "Branch Head", "branchKey": "saeed", "imageUrl": "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=200"},
            {"name": "Hassan Ali Al Hamad", "role": "Branch Head", "branchKey": "hamad", "imageUrl": "https://images.unsplash.com/photo-1612349317150-e413f6a5b16d?w=200"},
            {"name": "Abdullah Mohammed Al Naqool", "role": "Council President", "branchKey": "naqool", "imageUrl": "https://images.unsplash.com/photo-1560250097-0b93528c311a?w=200"},
            {"name": "Maha Saud Al Naqool", "role": "Council Secretary", "branchKey": "naqool", "imageUrl": "https://images.unsplash.com/photo-1580489944761-15a19d654956?w=200"},
        ],
        "founderLineage": [
            {"generation": 1, "name": "Naqool bin Abdullah", "subtitle": "Founder — early 19th century", "isFounder": True, "imageUrl": "https://images.unsplash.com/photo-1560250097-0b93528c311a?w=200"},
            {"generation": 2, "name": "Mohammed Al Naqool", "subtitle": "Eldest son — settled in Najd", "imageUrl": "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=200"},
            {"generation": 2, "name": "Ibrahim Al Naqool", "subtitle": "Founder of Al Dhafer line", "imageUrl": "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=200"},
            {"generation": 3, "name": "Salman Al Mofreh", "subtitle": "Al Mofreh branch patriarch", "imageUrl": "https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?w=200"},
            {"generation": 3, "name": "Fahad Al Saeed", "subtitle": "Medina line — archive keeper", "imageUrl": "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=200"},
            {"generation": 4, "name": "Abdullah Mohammed Al Naqool", "subtitle": "Current council president", "imageUrl": "https://images.unsplash.com/photo-1560250097-0b93528c311a?w=200"},
            {"generation": 5, "name": "Ahmed Abdullah Al Naqool", "subtitle": "5th generation — Al Mofreh", "imageUrl": "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=200"},
        ],
        "conversations": [
            {"key": "council", "name": "Family Council", "lastMessage": "Q2 agenda uploaded to Documents", "time": "10:30 AM", "unreadCount": 2, "isGroup": True, "type": "Announcements", "participantUserKeys": ["ahmed", "president", "mohammed", "noura"]},
            {"key": "mofreh", "name": "Al Mofreh Branch", "lastMessage": "Eid volunteer list — please confirm", "time": "Yesterday", "unreadCount": 0, "isGroup": True, "type": "Groups", "participantUserKeys": ["ahmed", "mohammed", "omar"]},
            {"key": "president_dm", "name": "Abdullah Al Naqool", "lastMessage": "Shukran, received the branch report", "time": "Mon", "unreadCount": 1, "isGroup": False, "type": "Direct", "participantUserKeys": ["ahmed", "president"]},
            {"key": "events", "name": "Events Committee", "lastMessage": "Venue contract signed for December gathering", "time": "Sun", "unreadCount": 0, "isGroup": True, "type": "Groups", "participantUserKeys": ["omar", "mohammed", "khalid"]},
            {"key": "youth", "name": "Youth Committee", "lastMessage": "Workshop registration closes Friday", "time": "Sat", "unreadCount": 3, "isGroup": True, "type": "Announcements", "participantUserKeys": ["omar", "ahmed"]},
        ],
        "chatMessages": [
            {"conversationKey": "council", "senderName": "Council Admin", "senderUserKey": None, "content": "Assalamu alaikum. Q2 meeting agenda is now in Documents.", "time": "9:00 AM", "isMe": False, "isAnnouncement": True},
            {"conversationKey": "council", "senderName": "Abdullah Al Naqool", "senderUserKey": "president", "content": "Thank you. All branch heads should review before Thursday.", "time": "9:20 AM", "isMe": False},
            {"conversationKey": "council", "senderName": "You", "senderUserKey": "ahmed", "content": "Al Mofreh branch confirms attendance.", "time": "9:45 AM", "isMe": True},
            {"conversationKey": "mofreh", "senderName": "Mohammed Al Mofreh", "senderUserKey": "mohammed", "content": "Volunteers needed for the July workshop — reply here.", "time": "Yesterday", "isMe": False},
            {"conversationKey": "mofreh", "senderName": "You", "senderUserKey": "ahmed", "content": "I can help with registration desk.", "time": "Yesterday", "isMe": True},
            {"conversationKey": "president_dm", "senderName": "Abdullah Al Naqool", "senderUserKey": "president", "content": "Ahmed, please send the updated Mofreh member count.", "time": "Mon", "isMe": False},
            {"conversationKey": "president_dm", "senderName": "You", "senderUserKey": "ahmed", "content": "Uploaded to Documents — 412 active members.", "time": "Mon", "isMe": True},
            {"conversationKey": "president_dm", "senderName": "Abdullah Al Naqool", "senderUserKey": "president", "content": "Shukran, received the branch report", "time": "Mon", "isMe": False},
            {"conversationKey": "events", "senderName": "Events Committee", "senderUserKey": "omar", "content": "December gathering venue confirmed — Diplomatic Quarter.", "time": "Sun", "isMe": False, "isAnnouncement": True},
            {"conversationKey": "youth", "senderName": "Omar Al Mofreh", "senderUserKey": "omar", "content": "Workshop registration closes this Friday.", "time": "Sat", "isMe": False},
        ],
        "albums": [
            {"key": "eid2025", "title": "Eid Gathering 2025 — Riyadh", "photoCount": 6, "imageUrl": "https://images.unsplash.com/photo-1758985776354-4df674930917?w=800", "isFeatured": True, "galleryTypeKey": "Photos", "description": "Annual Eid reunion at the Heritage Center."},
            {"key": "weddings", "title": "Wedding Celebrations", "photoCount": 4, "imageUrl": "https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=800", "isFeatured": False, "galleryTypeKey": "Photos", "description": "Weddings across branches 2024–2026."},
            {"key": "heritage", "title": "Heritage & Manuscripts", "photoCount": 3, "imageUrl": "https://images.unsplash.com/photo-1681419671941-aa9bc9df0bfb?w=800", "isFeatured": True, "galleryTypeKey": "Heritage", "description": "Archive digitization and old family photos."},
            {"key": "council2025", "title": "Council Meetings 2025", "photoCount": 2, "imageUrl": "https://images.unsplash.com/photo-1475721027785-f74eccf877e2?w=800", "isFeatured": False, "galleryTypeKey": "Albums", "description": "Official council sessions and signings."},
        ],
        "galleryPhotos": [
            {"albumKey": "eid2025", "caption": "Opening dua and welcome", "imageUrl": "https://images.unsplash.com/photo-1758985776354-4df674930917?w=800"},
            {"albumKey": "eid2025", "caption": "Family dinner — main hall", "imageUrl": "https://images.unsplash.com/photo-1511795409834-ef04bbd61622?w=800"},
            {"albumKey": "eid2025", "caption": "Group photo — all branches", "imageUrl": "https://images.unsplash.com/photo-1529156069898-49953e39b3ac?w=800"},
            {"albumKey": "eid2025", "caption": "Children's corner", "imageUrl": "https://images.unsplash.com/photo-1578662996442-48f60103fc96?w=800"},
            {"albumKey": "eid2025", "caption": "Heritage exhibition", "imageUrl": "https://images.unsplash.com/photo-1681419671941-aa9bc9df0bfb?w=800"},
            {"albumKey": "eid2025", "caption": "Evening closing", "imageUrl": "https://images.unsplash.com/photo-1770786106021-52580470e31e?w=800"},
            {"albumKey": "weddings", "caption": "Ahmed & Hessa reception", "imageUrl": "https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=800"},
            {"albumKey": "weddings", "caption": "Turki & Layla engagement", "imageUrl": "https://images.unsplash.com/photo-1517245386807-bb43f82c33c4?w=800"},
            {"albumKey": "heritage", "caption": "Original genealogy manuscript", "imageUrl": "https://images.unsplash.com/photo-1681419671941-aa9bc9df0bfb?w=800"},
            {"albumKey": "heritage", "caption": "Archive digitization team", "imageUrl": "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=800"},
            {"albumKey": "council2025", "caption": "Q4 2025 signing session", "imageUrl": "https://images.unsplash.com/photo-1475721027785-f74eccf877e2?w=800"},
        ],
        "documents": [
            {"title": "Al Naqool Family Genealogy — 2026 Edition", "fileSize": "4.2 MB", "date": "March 10, 2026", "category": "Family Records", "description": "Verified lineage across six branches with photos and migration notes.", "fileUrl": "/files/genealogy-2026.pdf", "accessLevel": "Members"},
            {"title": "Q1 2026 Council Minutes", "fileSize": "1.3 MB", "date": "April 8, 2026", "category": "Council", "description": "Approved decisions, task assignments, and attendance.", "fileUrl": "/files/minutes-q1-2026.pdf", "accessLevel": "Council"},
            {"title": "Family Constitution & Bylaws", "fileSize": "2.8 MB", "date": "January 15, 2026", "category": "Legal", "description": "Governing document for the Family Council and member conduct.", "fileUrl": "/files/constitution.pdf", "accessLevel": "Members"},
            {"title": "Annual Report 2025", "fileSize": "5.6 MB", "date": "December 20, 2025", "category": "Council", "description": "Activities, welfare fund summary, and branch highlights.", "fileUrl": "/files/annual-report-2025.pdf", "accessLevel": "Council"},
            {"title": "Youth Program Budget 2026", "fileSize": "890 KB", "date": "May 5, 2026", "category": "Financial", "description": "Proposed budget for youth leadership and scholarship programs.", "fileUrl": "/files/youth-budget-2026.pdf", "accessLevel": "Council"},
            {"title": "Guest Welcome Guide", "fileSize": "420 KB", "date": "June 1, 2026", "category": "Family Records", "description": "Introduction to branches and how to register as a guest.", "fileUrl": "/files/guest-guide.pdf", "accessLevel": "Public"},
        ],
        "councilModules": [
            {"id": "meetings", "iconName": "meeting", "label": "Meetings", "subtitle": "14 Total"},
            {"id": "committees", "iconName": "committee", "label": "Committees", "subtitle": "7 Active"},
            {"id": "voting", "iconName": "voting", "label": "Voting", "subtitle": "2 Open"},
            {"id": "tasks", "iconName": "tasks", "label": "Tasks", "subtitle": "6 Open"},
            {"id": "decisions", "iconName": "decisions", "label": "Decisions", "subtitle": "22 Recorded"},
            {"id": "members", "iconName": "members", "label": "Members", "subtitle": "28 Council"},
        ],
        "latestMeeting": {"title": "Q2 Council Meeting 2026", "date": "June 28, 2026", "time": "4:00 PM", "location": "Al Naqool Heritage Center, Riyadh", "decisions": 5, "tasks": 7, "attachments": 4, "minutesFileUrl": "/files/minutes-q2-2026-draft.pdf"},
        "notifications": [
            {"title": "Council Meeting Reminder", "body": "Q2 meeting on June 28 at 4:00 PM — Heritage Center.", "type": "Council", "isRead": False},
            {"title": "Youth Workshop Registration", "body": "You are registered for July 5 leadership workshop.", "type": "Events", "isRead": False},
            {"title": "New Document Available", "body": "Q1 2026 council minutes published.", "type": "Documents", "isRead": True},
            {"title": "Branch Message", "body": "Mohammed Al Mofreh posted in Al Mofreh group.", "type": "Messages", "isRead": True},
            {"title": "Wedding Announcement", "body": "Turki & Layla celebration — July 12 in Abha.", "type": "News", "isRead": False},
            {"title": "Registration Pending", "body": "New member Faisal Al Rashid awaiting approval.", "type": "Admin", "isRead": False},
        ],
        "directory": [
            {"name": "Mohammed Salman Al Mofreh", "role": "Branch Head", "branchKey": "mofreh", "phone": "+966501112233", "email": "mohammed.mofreh@alnaqool.com", "city": "Riyadh", "userKey": "mohammed"},
            {"name": "Sara Hamed Al Mofreh", "role": "Secretary", "branchKey": "mofreh", "phone": "+966502223344", "email": "sara.mofreh@alnaqool.com", "city": "Riyadh"},
            {"name": "Ahmed Abdullah Al Naqool", "role": "Family Member", "branchKey": "mofreh", "phone": "+966501234567", "email": "ahmed.alnaqool@email.com", "city": "Riyadh", "userKey": "ahmed"},
            {"name": "Khalid Ibrahim Al Dhafer", "role": "Branch Head", "branchKey": "dhafer", "phone": "+966504445566", "email": "khalid.dhafer@alnaqool.com", "city": "Dammam", "userKey": "khalid"},
            {"name": "Fatima Nasser Al Dhafer", "role": "Social Affairs", "branchKey": "dhafer", "phone": "+966505556677", "email": "fatima.dhafer@alnaqool.com", "city": "Khobar"},
            {"name": "Abdullah Turki Al Rashid", "role": "Branch Head", "branchKey": "rashid", "phone": "+966506667788", "email": "abdullah.rashid@alnaqool.com", "city": "Makkah"},
            {"name": "Noura Fahad Al Saeed", "role": "Branch Head", "branchKey": "saeed", "phone": "+966507778899", "email": "noura.saeed@alnaqool.com", "city": "Medina", "userKey": "noura"},
            {"name": "Hassan Ali Al Hamad", "role": "Branch Head", "branchKey": "hamad", "phone": "+966508889900", "email": "hassan.hamad@alnaqool.com", "city": "Abha"},
            {"name": "Abdullah Mohammed Al Naqool", "role": "Council President", "branchKey": "naqool", "phone": "+966509998877", "email": "abdullah.president@alnaqool.com", "city": "Riyadh", "userKey": "president"},
            {"name": "Maha Saud Al Naqool", "role": "Council Secretary", "branchKey": "naqool", "phone": "+966500112233", "email": "maha.naqool@alnaqool.com", "city": "Riyadh"},
            {"name": "Omar Mohammed Al Mofreh", "role": "Youth Representative", "branchKey": "mofreh", "phone": "+966503334455", "email": "omar.mofreh@alnaqool.com", "city": "Riyadh", "userKey": "omar"},
            {"name": "Faisal Turki Al Rashid", "role": "Applicant", "branchKey": "rashid", "phone": "+966506667788", "email": "faisal.rashid@gmail.com", "city": "Jeddah", "userKey": "pending_faisal"},
        ],
        "contactSubmissions": [
            {"name": "Layla Al Saeed", "email": "layla.saeed@email.com", "subject": "Wedding photo upload", "message": "How can I share photos from our engagement for the family gallery?", "isRead": False},
            {"name": "Guest User", "email": "visitor@example.com", "subject": "Membership inquiry", "message": "I believe I am related through the Al Rashid branch. How do I register?", "isRead": True},
            {"name": "Hassan Al Hamad", "email": "hassan.hamad@alnaqool.com", "subject": "Correct directory entry", "message": "Please update my phone number in the family directory.", "isRead": False},
        ],
        "passwordResetRequests": [
            {"userKey": "omar", "email": "omar.mofreh@alnaqool.com", "isResolved": False},
        ],
        "councilItems": {
            "meetings": [
                {"title": "Q2 Council Meeting 2026", "subtitle": "Heritage Center, Riyadh", "meta": "June 28, 2026 · 4:00 PM", "status": "Upcoming"},
                {"title": "Annual General Meeting", "subtitle": "Diplomatic Quarter", "meta": "December 18, 2026 · 6:00 PM", "status": "Scheduled"},
                {"title": "Q1 Council Meeting 2026", "subtitle": "Heritage Center", "meta": "March 22, 2026 · 4:00 PM", "status": "Completed"},
            ],
            "committees": [
                {"title": "Events Committee", "subtitle": "14 members", "meta": "Chair: Mohammed Al Mofreh", "status": "Active"},
                {"title": "Youth Committee", "subtitle": "11 members", "meta": "Chair: Omar Al Mofreh", "status": "Active"},
                {"title": "Heritage Committee", "subtitle": "9 members", "meta": "Chair: Noura Al Saeed", "status": "Active"},
                {"title": "Welfare Committee", "subtitle": "8 members", "meta": "Chair: Hassan Al Hamad", "status": "Active"},
            ],
            "voting": [
                {"title": "2026 Welfare Fund Budget", "subtitle": "Closes in 4 days", "meta": "156 votes cast", "status": "Open"},
                {"title": "Youth Scholarship Program", "subtitle": "Closes in 6 days", "meta": "112 votes cast", "status": "Open"},
            ],
            "tasks": [
                {"title": "Finalize Q2 meeting agenda", "subtitle": "Assigned to Maha Al Naqool", "meta": "Due June 25, 2026", "status": "In Progress"},
                {"title": "Update genealogy records", "subtitle": "Heritage Committee", "meta": "Due July 15, 2026", "status": "Open"},
                {"title": "Confirm December venue catering", "subtitle": "Events Committee", "meta": "Due August 1, 2026", "status": "Open"},
            ],
            "decisions": [
                {"title": "Approve December gathering venue", "subtitle": "Decision #2026-12", "meta": "Approved · June 10, 2026", "status": "Approved"},
                {"title": "Launch youth leadership program", "subtitle": "Decision #2026-11", "meta": "Approved · May 28, 2026", "status": "Approved"},
                {"title": "Eastern branch reunion date", "subtitle": "Decision #2026-10", "meta": "Approved · May 15, 2026", "status": "Approved"},
            ],
            "members": [
                {"title": "Abdullah Mohammed Al Naqool", "subtitle": "Council President", "meta": "Al Naqool", "status": "Active"},
                {"title": "Mohammed Salman Al Mofreh", "subtitle": "Branch Representative", "meta": "Al Mofreh", "status": "Active"},
                {"title": "Khalid Ibrahim Al Dhafer", "subtitle": "Branch Representative", "meta": "Al Dhafer", "status": "Active"},
                {"title": "Noura Fahad Al Saeed", "subtitle": "Council Secretary", "meta": "Al Saeed", "status": "Active"},
            ],
        },
    }


def ar_data():
    d = en_data()
    d["president"] = "عبدالله محمد النقول"
    d["landingSlides"] = [
        {"titleLine1": "جمعنا التراث،", "titleLine2": "وصلنا العائلة", "subtitle": "التطبيق الرسمي لعائلة النقول — أخبار وفعاليات وقرارات المجلس في مكان واحد."},
        {"titleLine1": "من الرياض إلى جدة،", "titleLine2": "عائلة واحدة ممتدة", "subtitle": "استكشف الفروع وتعرّف على الأعضاء وتابع نسبك عبر الأجيال."},
        {"titleLine1": "قرارات المجلس،", "titleLine2": "شفافة للجميع", "subtitle": "الاجتماعات واللجان والتصويت والوثائق — في متناول كل عضو."},
    ]
    d["newsCategories"] = ["الكل", "عام", "مواليد", "أعراس", "تعازي", "المجلس"]
    d["messageFilters"] = ["الكل", "إعلانات", "مجموعات", "خاص"]
    d["galleryTypes"] = ["صور", "فيديو", "ألبومات", "تراث"]
    d["documentCategories"] = ["الكل", "المجلس", "سجلات العائلة", "قانوني", "مالي"]
    d["directoryBranches"] = ["الكل", "المفرح", "الظافر", "الراشد", "السعيد", "الحماد", "النقول"]
    d["termsEn"] = d["termsEn"]
    d["termsAr"] = "بالتسجيل فإنك توافق على شروط تطبيق عائلة النقول وسياسة الخصوصية ولوائح المجلس. تُستخدم بيانات الأعضاء للتواصل العائلي فقط."
    branches_ar = [
        ("mofreh", "المفرح", 412, "أكبر الفروع ومقره الرياض. معروف بتنظيم التجمعات السنوية وبرامج الشباب."),
        ("dhafer", "الظافر", 318, "فرع المنطقة الشرقية وله دور بارز في التكافل وتنظيم التعازي."),
        ("rashid", "الراشد", 285, "فرع مكة النشط في المنح الدراسية وتنسيق موسم الحج."),
        ("saeed", "السعيد", 264, "فرع المدينة المنورة ويقود توثيق النسب والأرشيف الرقمي."),
        ("hamad", "الحماد", 231, "فرع الجنوب ويركز على صندوق الرعاية وكبار السن."),
        ("naqool", "النقول", 198, "خط النسب الأصلي — رئاسة المجلس والإدارة المركزية."),
    ]
    d["branches"] = [{"key": k, "name": n, "memberCount": c, "description": desc, "imageUrl": en_data()["branches"][i]["imageUrl"]} for i, (k, n, c, desc) in enumerate(branches_ar)]

    d["users"] = [
        {"key": "admin", "userName": "admin", "email": "admin@alnaqool.com", "password": "Admin123!", "fullName": "مدير النظام", "displayRole": "مدير", "memberId": "ADMIN-001", "branchKey": "naqool", "phone": "+966500000001", "role": "Admin", "accountStatus": "Active", "dateOfBirth": "1 يناير 1980", "maritalStatus": "متزوج", "childrenCount": 0},
        {"key": "ahmed", "userName": "ahmed", "email": "ahmed.alnaqool@email.com", "password": "password123", "fullName": "أحمد عبدالله النقول", "displayRole": "عضو العائلة", "memberId": "AN-2026-0847", "branchKey": "mofreh", "phone": "+966501234567", "role": "Member", "accountStatus": "Active", "dateOfBirth": "15 مارس 1985", "maritalStatus": "متزوج", "childrenCount": 3},
        {"key": "mohammed", "userName": "mohammed.mofreh", "email": "mohammed.mofreh@alnaqool.com", "password": "password123", "fullName": "محمد سلمان المفرح", "displayRole": "رئيس الفرع", "memberId": "AN-2026-0101", "branchKey": "mofreh", "phone": "+966501112233", "role": "BranchHead", "accountStatus": "Active", "dateOfBirth": "12 أغسطس 1972", "maritalStatus": "متزوج", "childrenCount": 5},
        {"key": "khalid", "userName": "khalid.dhafer", "email": "khalid.dhafer@alnaqool.com", "password": "password123", "fullName": "خالد إبراهيم الظافر", "displayRole": "رئيس الفرع", "memberId": "AN-2026-0201", "branchKey": "dhafer", "phone": "+966504445566", "role": "BranchHead", "accountStatus": "Active", "dateOfBirth": "3 مايو 1975", "maritalStatus": "متزوج", "childrenCount": 4},
        {"key": "noura", "userName": "noura.saeed", "email": "noura.saeed@alnaqool.com", "password": "password123", "fullName": "نورة فهد السعيد", "displayRole": "أمينة السر", "memberId": "AN-2026-0401", "branchKey": "saeed", "phone": "+966507778899", "role": "Secretary", "accountStatus": "Active", "dateOfBirth": "22 نوفمبر 1988", "maritalStatus": "متزوجة", "childrenCount": 2},
        {"key": "omar", "userName": "omar.mofreh", "email": "omar.mofreh@alnaqool.com", "password": "password123", "fullName": "عمر محمد المفرح", "displayRole": "منسق اللجان", "memberId": "AN-2026-0103", "branchKey": "mofreh", "phone": "+966503334455", "role": "CommitteeOrganizer", "accountStatus": "Active", "dateOfBirth": "9 يونيو 1995", "maritalStatus": "أعزب", "childrenCount": 0},
        {"key": "president", "userName": "abdullah.president", "email": "abdullah.president@alnaqool.com", "password": "password123", "fullName": "عبدالله محمد النقول", "displayRole": "رئيس المجلس", "memberId": "AN-2026-0001", "branchKey": "naqool", "phone": "+966509998877", "role": "CouncilPresident", "accountStatus": "Active", "dateOfBirth": "14 فبراير 1968", "maritalStatus": "متزوج", "childrenCount": 6},
        {"key": "pending_faisal", "userName": "faisal.new", "email": "faisal.rashid@gmail.com", "password": "password123", "fullName": "فيصل تركي الراشد", "displayRole": "عضو العائلة", "memberId": "PENDING-001", "branchKey": "rashid", "phone": "+966506667788", "role": "Member", "accountStatus": "Pending", "registrationRelation": "ابن عبدالله الراشد", "dateOfBirth": "18 أبريل 1998", "maritalStatus": "أعزب", "childrenCount": 0},
    ]

    news_ar = [
        ("المجلس", "اجتماع مجلس العائلة للربع الثاني — 28 يونيو", "نُشرت جدول أعمال الجلسة الربع سنوية في الرياض.", "يدعو مجلس العائلة ممثلي الفروع لاجتماع الربع الثاني في مركز تراث النقول. المواضيع: الميزانية السنوية وصندوق الشباب وتخطيط تجمع العيد."),
        ("أعراس", "زفاف تركي الحماد وليلى السعيد", "احتفال في أبها يوم 12 يوليو 2026.", "تهنئ العائلة تركي وليلى. حفل الرجال الساعة 9 مساءً وحفل النساء في قاعة الفيصلية. التأكيد عبر قسم الفعاليات."),
        ("مواليد", "ترحيب بريان — فرع المفرح", "إعلان ميلاد: طفل لأحمد وحصة المفرح.", "حمداً لله على سلامة ولادة ريان. الأم والطفل بخير. تفاصيل العقيقة يعلنها رئيس الفرع."),
        ("عام", "التجمع العائلي السنوي — احفظوا الموعد", "ضعوا 18 ديسمبر 2026 في التقويم.", "يعود التجمع هذا العام إلى الرياض مع معرض تراثي وركن أطفال وتكريم المساهمين."),
        ("تعازي", "تعازينا لفرع الظافر", "وفاة الشيخ إبراهيم الظافر — رحمه الله.", "يقدم المجلس التعازي لفرع الظافر. أوقات الصلاة والدفن تُشارك مع أعضاء الفرع."),
        ("المجلس", "إطلاق برنامج قيادة الشباب", "التقديم مفتوح للأعمار 18–30.", "تعلن لجنة الشباب عن برنامج لمدة ستة أشهر. آخر موعد للتقديم 1 يوليو."),
        ("عام", "دليل العائلة المحدّث 2026", "الدليل متاح الآن في التطبيق.", "رؤساء الفروع اعتمدوا بيانات التواصل. يمكن تحديث الملف الشخصي عبر الأمينة."),
        ("مواليد", "تهنئة — فرع الراشد", "توأم بنات لفيصل وريم الراشد.", "جود وليان. تطلب العائلة الدعاء وتشارك الفرح مع جميع الفروع."),
    ]
    en_news = en_data()["newsItems"]
    d["newsItems"] = []
    for i, (cat, title, desc, body) in enumerate(news_ar):
        e = en_news[i]
        d["newsItems"].append({**e, "category": cat, "title": title, "description": desc, "body": body, "publishedDate": ["20 يونيو 2026","18 يونيو 2026","15 يونيو 2026","10 يونيو 2026","5 يونيو 2026","28 مايو 2026","20 مايو 2026","12 مايو 2026"][i]})

    events_ar = [
        ("28", "يون", "اجتماع مجلس الربع الثاني", "مركز تراث النقول، الرياض", "4:00 م", "28 يونيو 2026", "جلسة ربع سنوية — مراجعة الميزانية وتقارير اللجان وتخطيط العيد.", "مجلس العائلة"),
        ("05", "يول", "ورشة الشباب — القيادة والتراث", "مركز الملك عبدالعزيز، الرياض", "10:00 ص", "5 يوليو 2026", "ورشة تفاعلية للأعمار 18–30. التسجيل مطلوب.", "لجنة الشباب"),
        ("12", "يول", "حفل زفاف تركي وليلى", "أبها — قاعة الفيصلية", "9:00 م", "12 يوليو 2026", "احتفال عائلي بزفاف تركي الحماد وليلى السعيد.", "اللجنة الاجتماعية"),
        ("20", "أغس", "يوم مفتوح للأرشيف التراثي", "المدينة — بيت آل سعيد", "5:00 م", "20 أغسطس 2026", "جولة في الأرشيف الرقمي مع لجنة التراث.", "لجنة التراث"),
        ("15", "سبت", "لقاء فرع المنطقة الشرقية", "مسرح الكورنيش، الدمام", "7:00 م", "15 سبتمبر 2026", "اللقاء السنوي لفرع الظافر — عشاء وأنشطة أطفال.", "فرع الظافر"),
        ("18", "ديس", "التجمع العائلي السنوي 2026", "الرياض — الحي الدبلوماسي", "6:00 م", "18 ديسمبر 2026", "التجمع السنوي الرئيسي لجميع الفروع.", "لجنة الفعاليات"),
    ]
    d["events"] = []
    for i, row in enumerate(events_ar):
        e = en_data()["events"][i]
        d["events"].append({**e, "day": row[0], "month": row[1], "title": row[2], "location": row[3], "time": row[4], "fullDate": row[5], "description": row[6], "organizer": row[7]})

    bm_ar = [
        ("محمد سلمان المفرح", "رئيس الفرع"), ("سارة حامد المفرح", "أمينة السر"), ("عمر محمد المفرح", "ممثل الشباب"),
        ("خالد إبراهيم الظافر", "رئيس الفرع"), ("فاطمة ناصر الظافر", "الشؤون الاجتماعية"),
        ("عبدالله تركي الراشد", "رئيس الفرع"), ("نورة فهد السعيد", "رئيسة الفرع"),
        ("حسن علي الحماد", "رئيس الفرع"), ("عبدالله محمد النقول", "رئيس المجلس"), ("مها سعود النقول", "أمينة سر المجلس"),
    ]
    d["branchMembers"] = [{**en_data()["branchMembers"][i], "name": bm_ar[i][0], "role": bm_ar[i][1]} for i in range(len(bm_ar))]

    fl_ar = [
        ("نقول بن عبدالله", "المؤسس — أوائل القرن التاسع عشر"),
        ("محمد النقول", "الابن الأكبر — استقر في نجد"),
        ("إبراهيم النقول", "مؤسس خط الظافر"),
        ("سلمان المفرح", "كبير عائلة المفرح"),
        ("فهد السعيد", "خط المدينة — حارس الأرشيف"),
        ("عبدالله محمد النقول", "رئيس المجلس الحالي"),
        ("أحمد عبدالله النقول", "الجيل الخامس — المفرح"),
    ]
    d["founderLineage"] = [{**en_data()["founderLineage"][i], "name": fl_ar[i][0], "subtitle": fl_ar[i][1]} for i in range(len(fl_ar))]

    conv_ar = [
        ("council", "مجلس العائلة", "تم رفع جدول الربع الثاني في الوثائق"),
        ("mofreh", "فرع المفرح", "قائمة متطوعي العيد — يرجى التأكيد"),
        ("president_dm", "عبدالله النقول", "شكراً، وصلني تقرير الفرع"),
        ("events", "لجنة الفعاليات", "تم توقيع عقد قاعة ديسمبر"),
        ("youth", "لجنة الشباب", "إغلاق التسجيل للورشة يوم الجمعة"),
    ]
    d["conversations"] = []
    for i, (k, name, last) in enumerate(conv_ar):
        d["conversations"].append({**en_data()["conversations"][i], "key": k, "name": name, "lastMessage": last})

    msg_ar = [
        ("council", "إدارة المجلس", None, "السلام عليكم. جدول اجتماع الربع الثاني في الوثائق."),
        ("council", "عبدالله النقول", "president", "شكراً. على رؤساء الفروع المراجعة قبل الخميس."),
        ("council", "أنت", "ahmed", "فرع المفرح يؤكد الحضور."),
        ("mofreh", "محمد المفرح", "mohammed", "نحتاج متطوعين لورشة يوليو — ردوا هنا."),
        ("mofreh", "أنت", "ahmed", "أستطيع المساعدة في مكتب التسجيل."),
        ("president_dm", "عبدالله النقول", "president", "أحمد، أرسل عدد أعضاء المفرح المحدّث."),
        ("president_dm", "أنت", "ahmed", "رفعته في الوثائق — 412 عضواً نشطاً."),
        ("president_dm", "عبدالله النقول", "president", "شكراً، وصلني تقرير الفرع"),
        ("events", "لجنة الفعاليات", "omar", "تم تأكيد قاعة ديسمبر — الحي الدبلوماسي."),
        ("youth", "عمر المفرح", "omar", "إغلاق تسجيل الورشة يوم الجمعة."),
    ]
    d["chatMessages"] = []
    for i, (ck, sender, uk, content) in enumerate(msg_ar):
        m = {**en_data()["chatMessages"][i], "conversationKey": ck, "senderName": sender, "senderUserKey": uk, "content": content}
        m["isMe"] = sender == "أنت"
        d["chatMessages"].append(m)

    albums_ar = [
        ("eid2025", "تجمع العيد 2025 — الرياض", "التجمع السنوي في مركز التراث."),
        ("weddings", "أفراح العائلة", "أعراس الفروع 2024–2026."),
        ("heritage", "التراث والمخطوطات", "رقمنة الأرشيف وصور قديمة."),
        ("council2025", "اجتماعات المجلس 2025", "جلسات المجلس والتوقيعات الرسمية."),
    ]
    d["albums"] = [{**en_data()["albums"][i], "key": albums_ar[i][0], "title": albums_ar[i][1], "description": albums_ar[i][2]} for i in range(4)]

    caps_ar = ["الافتتاح والدعاء", "عشاء العائلة — القاعة الرئيسية", "صورة جماعية — جميع الفروع", "ركن الأطفال", "معرض التراث", "ختام المساء", "حفل أحمد وحصة", "خطوبة تركي وليلى", "مخطوطة النسب الأصلية", "فريق رقمنة الأرشيف", "جلسة توقيع الربع الرابع 2025"]
    d["galleryPhotos"] = [{**en_data()["galleryPhotos"][i], "caption": caps_ar[i]} for i in range(len(caps_ar))]

    docs_ar = [
        ("شجرة عائلة النقول — إصدار 2026", "سجل نسب موثّق لستة فروع مع صور وملاحظات.", "10 مارس 2026"),
        ("محضر مجلس الربع الأول 2026", "قرارات معتمدة ومهام وحضور.", "8 أبريل 2026"),
        ("النظام الأساسي ولوائح العائلة", "وثيقة حوكمة مجلس العائلة وسلوك الأعضاء.", "15 يناير 2026"),
        ("التقرير السنوي 2025", "أنشطة وصندوق الرعاية وإنجازات الفروع.", "20 ديسمبر 2025"),
        ("ميزانية برنامج الشباب 2026", "مقترح ميزانية المنح وبرامج القيادة.", "5 مايو 2026"),
        ("دليل الترحيب للضيوف", "تعريف بالفروع وكيفية التسجيل كضيف.", "1 يونيو 2026"),
    ]
    d["documents"] = []
    for i, (title, desc, date) in enumerate(docs_ar):
        d["documents"].append({**en_data()["documents"][i], "title": title, "description": desc, "date": date})

    d["councilModules"] = [
        {"id": "meetings", "iconName": "meeting", "label": "الاجتماعات", "subtitle": "14 إجمالي"},
        {"id": "committees", "iconName": "committee", "label": "اللجان", "subtitle": "7 نشطة"},
        {"id": "voting", "iconName": "voting", "label": "التصويت", "subtitle": "2 مفتوحة"},
        {"id": "tasks", "iconName": "tasks", "label": "المهام", "subtitle": "6 مفتوحة"},
        {"id": "decisions", "iconName": "decisions", "label": "القرارات", "subtitle": "22 مسجلة"},
        {"id": "members", "iconName": "members", "label": "الأعضاء", "subtitle": "28 عضو مجلس"},
    ]
    d["latestMeeting"] = {"title": "اجتماع مجلس الربع الثاني 2026", "date": "28 يونيو 2026", "time": "4:00 م", "location": "مركز تراث النقول، الرياض", "decisions": 5, "tasks": 7, "attachments": 4, "minutesFileUrl": "/files/minutes-q2-2026-draft.pdf"}

    notif_ar = [
        ("تذكير اجتماع المجلس", "اجتماع الربع الثاني 28 يونيو الساعة 4 مساءً — مركز التراث."),
        ("تسجيل ورشة الشباب", "أنت مسجّل في ورشة القيادة يوم 5 يوليو."),
        ("وثيقة جديدة", "نُشر محضر مجلس الربع الأول 2026."),
        ("رسالة فرعية", "محمد المفرح نشر في مجموعة المفرح."),
        ("إعلان زفاف", "احتفال تركي وليلى — 12 يوليو في أبها."),
        ("طلب تسجيل معلّق", "فيصل الراشد بانتظار الموافقة."),
    ]
    d["notifications"] = [{**en_data()["notifications"][i], "title": notif_ar[i][0], "body": notif_ar[i][1]} for i in range(6)]

    dir_ar = [
        ("محمد سلمان المفرح", "رئيس الفرع", "الرياض"), ("سارة حامد المفرح", "أمينة السر", "الرياض"),
        ("أحمد عبدالله النقول", "عضو العائلة", "الرياض"), ("خالد إبراهيم الظافر", "رئيس الفرع", "الدمام"),
        ("فاطمة ناصر الظافر", "الشؤون الاجتماعية", "الخبر"), ("عبدالله تركي الراشد", "رئيس الفرع", "مكة"),
        ("نورة فهد السعيد", "رئيسة الفرع", "المدينة"), ("حسن علي الحماد", "رئيس الفرع", "أبها"),
        ("عبدالله محمد النقول", "رئيس المجلس", "الرياض"), ("مها سعود النقول", "أمينة سر المجلس", "الرياض"),
        ("عمر محمد المفرح", "ممثل الشباب", "الرياض"), ("فيصل تركي الراشد", "مقدم طلب", "جدة"),
    ]
    d["directory"] = []
    for i, (name, role, city) in enumerate(dir_ar):
        entry = {**en_data()["directory"][i], "name": name, "role": role, "city": city}
        branch_names = {"mofreh": "المفرح", "dhafer": "الظافر", "rashid": "الراشد", "saeed": "السعيد", "hamad": "الحماد", "naqool": "النقول"}
        entry["branchName"] = branch_names.get(entry["branchKey"], "")
        d["directory"].append(entry)

    d["contactSubmissions"] = [
        {"name": "ليلى السعيد", "email": "layla.saeed@email.com", "subject": "رفع صور الزفاف", "message": "كيف أشارك صور الخطوبة في معرض العائلة؟", "isRead": False},
        {"name": "زائر", "email": "visitor@example.com", "subject": "استفسار عضوية", "message": "أعتقد أنني من فرع الراشد. كيف أسجّل؟", "isRead": True},
        {"name": "حسن الحماد", "email": "hassan.hamad@alnaqool.com", "subject": "تصحيح بيانات الدليل", "message": "يرجى تحديث رقم جوالي في دليل العائلة.", "isRead": False},
    ]

    # council items Arabic
    ci = en_data()["councilItems"]
    d["councilItems"] = {
        "meetings": [
            {"title": "اجتماع مجلس الربع الثاني 2026", "subtitle": "مركز التراث، الرياض", "meta": "28 يونيو 2026 · 4:00 م", "status": "قادم"},
            {"title": "الجمعية العمومية السنوية", "subtitle": "الحي الدبلوماسي", "meta": "18 ديسمبر 2026 · 6:00 م", "status": "مجدول"},
            {"title": "اجتماع الربع الأول 2026", "subtitle": "مركز التراث", "meta": "22 مارس 2026 · 4:00 م", "status": "مكتمل"},
        ],
        "committees": [
            {"title": "لجنة الفعاليات", "subtitle": "14 عضواً", "meta": "الرئيس: محمد المفرح", "status": "نشطة"},
            {"title": "لجنة الشباب", "subtitle": "11 عضواً", "meta": "الرئيس: عمر المفرح", "status": "نشطة"},
            {"title": "لجنة التراث", "subtitle": "9 أعضاء", "meta": "الرئيسة: نورة السعيد", "status": "نشطة"},
            {"title": "لجنة الرعاية", "subtitle": "8 أعضاء", "meta": "الرئيس: حسن الحماد", "status": "نشطة"},
        ],
        "voting": [
            {"title": "ميزانية صندوق الرعاية 2026", "subtitle": "يغلق خلال 4 أيام", "meta": "156 صوتاً", "status": "مفتوح"},
            {"title": "برنامج منح الشباب", "subtitle": "يغلق خلال 6 أيام", "meta": "112 صوتاً", "status": "مفتوح"},
        ],
        "tasks": [
            {"title": "إنهاء جدول اجتماع الربع الثاني", "subtitle": "مكلفة: مها النقول", "meta": "25 يونيو 2026", "status": "قيد التنفيذ"},
            {"title": "تحديث سجلات النسب", "subtitle": "لجنة التراث", "meta": "15 يوليو 2026", "status": "مفتوحة"},
            {"title": "تأكيد catering ديسمبر", "subtitle": "لجنة الفعاليات", "meta": "1 أغسطس 2026", "status": "مفتوحة"},
        ],
        "decisions": [
            {"title": "اعتماد قاعة تجمع ديسمبر", "subtitle": "قرار #2026-12", "meta": "معتمد · 10 يونيو 2026", "status": "معتمد"},
            {"title": "إطلاق برنامج قيادة الشباب", "subtitle": "قرار #2026-11", "meta": "معتمد · 28 مايو 2026", "status": "معتمد"},
            {"title": "موعد لقاء فرع الشرقية", "subtitle": "قرار #2026-10", "meta": "معتمد · 15 مايو 2026", "status": "معتمد"},
        ],
        "members": [
            {"title": "عبدالله محمد النقول", "subtitle": "رئيس المجلس", "meta": "النقول", "status": "نشط"},
            {"title": "محمد سلمان المفرح", "subtitle": "ممثل الفرع", "meta": "المفرح", "status": "نشط"},
            {"title": "خالد إبراهيم الظافر", "subtitle": "ممثل الفرع", "meta": "الظافر", "status": "نشط"},
            {"title": "نورة فهد السعيد", "subtitle": "أمينة سر المجلس", "meta": "السعيد", "status": "نشطة"},
        ],
    }
    return d


def strip_internal(obj):
  if isinstance(obj, dict):
    return {k: strip_internal(v) for k, v in obj.items() if not k.endswith("Key") or k in ("galleryTypeKey", "committeeKey", "typeKey")}
  if isinstance(obj, list):
    return [strip_internal(x) for x in obj]
  return obj

# Keep keys in JSON for seeder - don't strip

if __name__ == "__main__":
    en = en_data()
    ar = ar_data()
    (OUT / "en.json").write_text(json.dumps(en, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    (OUT / "ar.json").write_text(json.dumps(ar, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print("Wrote en.json and ar.json")
