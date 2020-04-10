clc
clear

data = importdata('data.txt');
target = importdata('target.txt');
all = importdata('all_data.txt');

size = length(data(:,1));
ae = zeros(size-1,2);

%A = pi/4.*target(:,1);

for i = 1:size-1
   
    num = data(i+1,1);
    D(i) = sqrt((data(i,3)-target(num,2))^2 + (data(i,4)-target(num,3))^2);
    MT(i) = data(i+1,2) - data(i,2);
    
    A = pi/4*target(num,1);
    ID(i) = log2(D(i)/A + 1);
    
    xo = [data(i,3),data(i,4)];
    xf = [data(i+1,3),data(i+1,4)];
    xd = [target(num,2),target(num,3)];
    
    ae(i,:) = dot(xf-xo,xd-xo)/dot(xd-xo,xd-xo)*(xd-xo)'; %effective amplitude
    dx(i) = norm(ae(i,:)) - norm(xd-xo);
    
    xt(i) = norm(xd-xo);
    
end


TP = log2(mean(vecnorm(ae'))/(4.1333*std(dx)) + 1)./mean(MT);
%ID = log2(D./A' + 1);

X = [ID',ones(size-1,1)];
Y = MT';
b = X\Y;

rsquared = corrcoef(ID,MT);

figure
plot(ID,MT,'*')
hold on
plot([0 7],b'*[0 7; 1 1])
ylabel('MT (s)')
xlabel('ID (bits)')
txt = 'r = 0.75';
text(1,1,txt);


figure
plot(all.data(166:215,1),all.data(166:215,2))
hold on
plot(992.1773, 382.5222, '*')
%plot([0 7],b'*[0 7; 1 1])
ylabel('Y (px)')
xlabel('X (px)')

param = [data(2,3),1; 992.1773,1]\[data(2,4);382.5222];

j = 1;
for i = linspace(0,2*pi,100);
   circ1(j,1) = 992.1773 + 18.74117*cos(i);
   circ1(j,2) = 382.5222 + 18.74117*sin(i);
   j = j+1;
end
 

figure
plot(data(2:3,3),data(2:3,4),'LineWidth',1.25)
hold on
plot(992.1773, 382.5222, '*')
plot([data(2,3),992.1773], [data(2,4),382.5222], '-','LineWidth',1.25)
plot([992.1773,984], [382.5222,param'*[984;1]], '-','LineWidth',1.25)
plot(circ1(:,1),circ1(:,2))
%plot([0 7],b'*[0 7; 1 1])
ylabel('Y (px)')
xlabel('X (px)')
%axis([ 950 1300 200 550])
