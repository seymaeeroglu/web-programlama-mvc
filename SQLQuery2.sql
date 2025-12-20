-- 1. ÖNCE SİSTEMDE OLMAYAN HAVALI BRANŞLARI EKLEYELİM
-- Tablo adı: Uzmanliklar
INSERT INTO Uzmanliklar (Ad) VALUES 
('Spinning'),
('Tenis'),
('Dans (Latin)'),
('Basketbol'),
('Boks'),
('Meditasyon');

-- 2. ŞİMDİ BU YENİ BRANŞLARA HİZMET (FİYAT/SÜRE) EKLEYELİM
-- (ID'leri 'Uzmanliklar' tablosundan bulup ekleyen güvenli yöntem)

-- Spinning: 45 dk, 300 TL
INSERT INTO Hizmetler (UzmanlikId, SureDk, Ucret) 
VALUES ((SELECT Id FROM Uzmanliklar WHERE Ad = 'Spinning'), 45, 300);

-- Tenis: 60 dk, 750 TL
INSERT INTO Hizmetler (UzmanlikId, SureDk, Ucret) 
VALUES ((SELECT Id FROM Uzmanliklar WHERE Ad = 'Tenis'), 60, 750);

-- Dans: 60 dk, 250 TL
INSERT INTO Hizmetler (UzmanlikId, SureDk, Ucret) 
VALUES ((SELECT Id FROM Uzmanliklar WHERE Ad = 'Dans (Latin)'), 60, 250);

-- Basketbol: 90 dk, 400 TL
INSERT INTO Hizmetler (UzmanlikId, SureDk, Ucret) 
VALUES ((SELECT Id FROM Uzmanliklar WHERE Ad = 'Basketbol'), 90, 400);

-- Boks: 50 dk, 500 TL
INSERT INTO Hizmetler (UzmanlikId, SureDk, Ucret) 
VALUES ((SELECT Id FROM Uzmanliklar WHERE Ad = 'Boks'), 50, 500);

-- Meditasyon: 30 dk, 150 TL
INSERT INTO Hizmetler (UzmanlikId, SureDk, Ucret) 
VALUES ((SELECT Id FROM Uzmanliklar WHERE Ad = 'Meditasyon'), 30, 150);